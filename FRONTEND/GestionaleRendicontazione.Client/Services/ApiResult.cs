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

        /// <summary>Codice HTTP della risposta (0 se la richiesta non è partita, es. network error).</summary>
        public int StatusCode { get; private init; }

        /// <summary>Errori di validazione: chiave = nome campo (camelCase), value = lista messaggi.</summary>
        public Dictionary<string, string[]> ValidationErrors { get; private init; } = new();

        /// <summary>Messaggio d'errore generico (non-400).</summary>
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

    /// <summary>Risultato senza body (es. Delete che ritorna 204).</summary>
    public sealed class ApiResult
    {
        public bool IsSuccess { get; private init; }

        /// <summary>Codice HTTP della risposta (0 se la richiesta non è partita, es. network error).</summary>
        public int StatusCode { get; private init; }

        public Dictionary<string, string[]> ValidationErrors { get; private init; } = new();
        public string? ErrorMessage { get; private init; }

        public static ApiResult Ok(int statusCode = 200) => new() { IsSuccess = true, StatusCode = statusCode };
        public static ApiResult WithValidationErrors(Dictionary<string, string[]> errors, int statusCode = 400)
            => new() { IsSuccess = false, ValidationErrors = errors, StatusCode = statusCode };
        public static ApiResult WithError(string message, int statusCode = 500)
            => new() { IsSuccess = false, ErrorMessage = message, StatusCode = statusCode };
    }

    /// <summary>
    /// Metodi helper per deserializzare HttpResponseMessage → ApiResult e per
    /// trasformare i risultati in stringhe leggibili dall'utente finale.
    /// </summary>
    public static class ApiResultExtensions
    {
        /// <summary>
        /// Mappa uno <see cref="ApiResult"/> a un messaggio leggibile in italiano per l'utente finale.
        /// Se il risultato contiene errori di validazione (400), li restituisce concatenati;
        /// altrimenti mappa lo StatusCode a un testo contestuale.
        /// </summary>
        public static string ToUserMessage(this ApiResult result, string? fallback = null)
        {
            return ToUserMessage(result.StatusCode, result.ValidationErrors, result.ErrorMessage, fallback);
        }

        /// <summary>
        /// Overload generico per <see cref="ApiResult{T}"/>: riusa la stessa logica di
        /// mapping dell'overload non generico.
        /// </summary>
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
            // 400 con validation errors: meglio mostrare i messaggi specifici del backend
            if (statusCode == 400 && validationErrors.Count > 0)
                return string.Join(" ", validationErrors.SelectMany(kv => kv.Value));

            // 400 senza validation errors: messaggio generico sul 400
            if (statusCode == 400)
                return "Alcuni dati non sono validi. Controlla i campi evidenziati.";

            // Status code diverso da 400 ma con validation errors popolati (backend non conforme):
            // mostriamo comunque i messaggi specifici invece del testo generico per status code.
            if (validationErrors.Count > 0)
                return string.Join(" ", validationErrors.SelectMany(kv => kv.Value));

            // 401 viene gestito da AuthenticatedHttpMessageHandler prima di arrivare qui,
            // ma se capita (es. con un client che non usa l'handler) mostriamo un fallback decente
            if (statusCode == 401)
                return "Sessione scaduta. Accedi di nuovo per continuare.";

            if (statusCode == 403)
                return "Non hai i permessi per eseguire questa operazione.";

            if (statusCode == 404)
                return "Risorsa non trovata.";

            if (statusCode == 409)
            {
                // Se il backend ha popolato ProblemDetails.Detail con un messaggio
                // specifico (es. "Impossibile eliminare il progetto 'X': ..."), usiamo
                // quello: è già il wording user-facing voluto. Altrimenti ricadiamo sul
                // fallback generico.
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
                // Anche per i 5xx mostriamo il detail del backend quando c'è (es. per
                // InvalidOperationException non mappata): l'utente finale vede comunque
                // il problema reale, non un generico "riprova più tardi".
                if (!string.IsNullOrWhiteSpace(errorMessage))
                    return errorMessage;
                return "Il server ha risposto con un errore. Riprova più tardi.";
            }

            // StatusCode == 0: la richiesta non è partita (network/down/timeout/CORS)
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

            // Per tutti gli altri errori (404/409/422/500/...): prova a leggere il ProblemDetails
            // che il backend costruisce con Detail = exception.Message. Se la lettura riesce,
            // il messaggio user-facing arriva dal server, NON da un fallback generico lato client.
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

        internal static async Task<string?> TryReadProblemDetailAsync(
            HttpResponseMessage response, CancellationToken ct = default)
        {
            try
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                using var doc = JsonDocument.Parse(json);

                // RFC 7807: "detail" è il messaggio human-readable; "title" è la categoria.
                // Preferiamo detail quando c'è (è quello che il backend ha popolato con
                // exception.Message), altrimenti ricadiamo su title.
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
                // body non era JSON ProblemDetails (o era vuoto): lasciamo null
                // e ToUserMessage userà il fallback per status code.
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
