using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GestionaleRendicontazione.Client.Services
{
    public sealed class ApiResult<T>
    {
        public bool IsSuccess { get; private init; }
        public T? Data { get; private init; }

        public int StatusCode { get; private init; }

        public Dictionary<string, string[]> ValidationErrors { get; private init; } = new();

        public string? ErrorMessage { get; private init; }

        public static ApiResult<T> Ok(T data, int statusCode = 200) => new()
        {
            IsSuccess = true,
            Data = data,
            StatusCode = statusCode,
        };

        public static ApiResult<T> WithValidationErrors(Dictionary<string, string[]> errors, int statusCode = 400)
            => new()
            {
                IsSuccess = false,
                ValidationErrors = errors,
                StatusCode = statusCode,
            };

        public static ApiResult<T> WithError(string message, int statusCode = 500)
            => new()
            {
                IsSuccess = false,
                ErrorMessage = message,
                StatusCode = statusCode,
            };
    }

    public sealed class ApiResult
    {
        public bool IsSuccess { get; private init; }

        public int StatusCode { get; private init; }

        public Dictionary<string, string[]> ValidationErrors { get; private init; } = new();
        public string? ErrorMessage { get; private init; }

        public static ApiResult Ok(int statusCode = 200) => new() { IsSuccess = true, StatusCode = statusCode };
        public static ApiResult WithValidationErrors(Dictionary<string, string[]> errors, int statusCode = 400)
            => new() { IsSuccess = false, ValidationErrors = errors, StatusCode = statusCode };
        public static ApiResult WithError(string message, int statusCode = 500)
            => new() { IsSuccess = false, ErrorMessage = message, StatusCode = statusCode };
    }

    public static class ApiResultExtensions
    {
        public static string ToUserMessage(this ApiResult result, string? fallback = null)
        {
            return ToUserMessage(result.StatusCode, result.ValidationErrors, result.ErrorMessage, fallback);
        }

        public static string ToUserMessage<T>(this ApiResult<T> result, string? fallback = null)
        {
            return ToUserMessage(result.StatusCode, result.ValidationErrors, result.ErrorMessage, fallback);
        }

        internal static string ToUserMessage(
            int statusCode,
            Dictionary<string, string[]> validationErrors,
            string? errorMessage,
            string? fallback)
        {
            if (statusCode == 400 && validationErrors.Count > 0)
                return string.Join(" ", validationErrors.SelectMany(kv => kv.Value));

            if (statusCode == 400)
                return "Alcuni dati non sono validi. Controlla i campi evidenziati.";

            if (statusCode == 401)
                return "Sessione scaduta. Accedi di nuovo per continuare.";

            if (statusCode == 403)
                return "Non hai i permessi per eseguire questa operazione.";

            if (statusCode == 404)
                return "Risorsa non trovata.";

            if (statusCode == 409)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                    return errorMessage;
                return "Impossibile completare l'operazione: l'elemento è referenziato da altri dati collegati. Rimuovi prima i riferimenti e riprova.";
            }

            if (statusCode == 422)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                    return errorMessage;
                return "I dati inviati non sono elaborabili dal server.";
            }

            if (statusCode >= 500 && statusCode < 600)
            {
                if (!string.IsNullOrWhiteSpace(errorMessage))
                    return errorMessage;
                return "Il server ha risposto con un errore. Riprova più tardi.";
            }

            if (statusCode == 0)
                return "Impossibile contattare il server. Verifica la connessione.";

            return errorMessage ?? fallback ?? "Si è verificato un errore imprevisto.";
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public static async Task<ApiResult<T>> ToApiResultAsync<T>(
            this HttpResponseMessage response,
            CancellationToken ct = default)
        {
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>(_jsonOptions, ct);
                return ApiResult<T>.Ok(data!, (int)response.StatusCode);
            }

            if ((int)response.StatusCode == 400)
            {
                var problem = await TryReadValidationProblemAsync(response, ct);
                if (problem is not null)
                    return ApiResult<T>.WithValidationErrors(problem, (int)response.StatusCode);
            }

            var detail = await TryReadProblemDetailAsync(response, ct) ?? $"Errore HTTP {(int)response.StatusCode}";
            return ApiResult<T>.WithError(detail, (int)response.StatusCode);
        }

        public static async Task<ApiResult> ToApiResultAsync(
            this HttpResponseMessage response,
            CancellationToken ct = default)
        {
            if (response.IsSuccessStatusCode)
                return ApiResult.Ok((int)response.StatusCode);

            if ((int)response.StatusCode == 400)
            {
                var problem = await TryReadValidationProblemAsync(response, ct);
                if (problem is not null)
                    return ApiResult.WithValidationErrors(problem, (int)response.StatusCode);
            }

            var detail = await TryReadProblemDetailAsync(response, ct) ?? $"Errore HTTP {(int)response.StatusCode}";
            return ApiResult.WithError(detail, (int)response.StatusCode);
        }

        private static async Task<string?> TryReadProblemDetailAsync(
            HttpResponseMessage response, CancellationToken ct)
        {
            try
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("detail", out var detailEl)
                    && detailEl.ValueKind == JsonValueKind.String)
                {
                    var detail = detailEl.GetString();
                    if (!string.IsNullOrWhiteSpace(detail)) return detail;
                }
                if (doc.RootElement.TryGetProperty("title", out var titleEl)
                    && titleEl.ValueKind == JsonValueKind.String)
                {
                    var title = titleEl.GetString();
                    if (!string.IsNullOrWhiteSpace(title)) return title;
                }
            }
            catch
            {
            }
            return null;
        }

        private static async Task<Dictionary<string, string[]>?> TryReadValidationProblemAsync(
            HttpResponseMessage response, CancellationToken ct)
        {
            try
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("errors", out var errorsEl))
                    return null;

                var dict = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
                foreach (var prop in errorsEl.EnumerateObject())
                {
                    var msgs = prop.Value.EnumerateArray()
                        .Select(e => e.GetString() ?? string.Empty)
                        .ToArray();
                    dict[prop.Name] = msgs;
                }
                return dict;
            }
            catch
            {
                return null;
            }
        }

        public static bool ApplyTo(
            this Dictionary<string, string[]> validationErrors,
            Dictionary<string, string> targetFieldErrors)
        {
            targetFieldErrors.Clear();
            foreach (var (field, messages) in validationErrors)
                targetFieldErrors[field] = string.Join(" ", messages);
            return targetFieldErrors.Count > 0;
        }
    }
}
