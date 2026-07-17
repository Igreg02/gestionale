using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Risultato tipizzato per le chiamate HTTP verso le API REST.
    /// Gestisce in modo centralizzato:
    ///  - successo con body deserializzato
    ///  - errori 400/ValidationProblemDetails (mappa campo→messaggi)
    ///  - altri errori HTTP
    /// </summary>
    public sealed class ApiResult<T>
    {
        public bool IsSuccess { get; private init; }
        public T? Data { get; private init; }

        /// <summary>Errori di validazione: chiave = nome campo (camelCase), value = lista messaggi.</summary>
        public Dictionary<string, string[]> ValidationErrors { get; private init; } = new();

        /// <summary>Messaggio d'errore generico (non-400).</summary>
        public string? ErrorMessage { get; private init; }

        public static ApiResult<T> Ok(T data) => new() { IsSuccess = true, Data = data };

        public static ApiResult<T> WithValidationErrors(Dictionary<string, string[]> errors)
            => new() { IsSuccess = false, ValidationErrors = errors };

        public static ApiResult<T> WithError(string message)
            => new() { IsSuccess = false, ErrorMessage = message };
    }

    /// <summary>Risultato senza body (es. Delete che ritorna 204).</summary>
    public sealed class ApiResult
    {
        public bool IsSuccess { get; private init; }
        public Dictionary<string, string[]> ValidationErrors { get; private init; } = new();
        public string? ErrorMessage { get; private init; }

        public static ApiResult Ok() => new() { IsSuccess = true };
        public static ApiResult WithValidationErrors(Dictionary<string, string[]> errors)
            => new() { IsSuccess = false, ValidationErrors = errors };
        public static ApiResult WithError(string message)
            => new() { IsSuccess = false, ErrorMessage = message };
    }

    /// <summary>
    /// Metodi helper per deserializzare HttpResponseMessage → ApiResult.
    /// </summary>
    public static class ApiResultExtensions
    {
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
                return ApiResult<T>.Ok(data!);
            }

            if ((int)response.StatusCode == 400)
            {
                var problem = await TryReadValidationProblemAsync(response, ct);
                if (problem is not null)
                    return ApiResult<T>.WithValidationErrors(problem);
            }

            return ApiResult<T>.WithError($"Errore HTTP {(int)response.StatusCode}");
        }

        public static async Task<ApiResult> ToApiResultAsync(
            this HttpResponseMessage response,
            CancellationToken ct = default)
        {
            if (response.IsSuccessStatusCode)
                return ApiResult.Ok();

            if ((int)response.StatusCode == 400)
            {
                var problem = await TryReadValidationProblemAsync(response, ct);
                if (problem is not null)
                    return ApiResult.WithValidationErrors(problem);
            }

            return ApiResult.WithError($"Errore HTTP {(int)response.StatusCode}");
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

        /// <summary>
        /// Applica gli errori di validazione a un dizionario campo→messaggio usabile dal form.
        /// Restituisce true se ci sono errori.
        /// </summary>
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
