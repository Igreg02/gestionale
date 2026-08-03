using System.Net.Http.Json;
using GestionaleRendicontazione.Client.Models;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Client tipizzato per l'endpoint <c>api/Log</c>. Solo lettura (GET paginata con filtri):
    /// i log sono scritti esclusivamente dal sink Serilog lato server. Riservato al ruolo Admin
    /// (il token viene allegato automaticamente da <see cref="AuthenticatedHttpMessageHandler"/>).
    /// </summary>
    public sealed class LogApiClient
    {
        private readonly HttpClient _httpClient;

        public LogApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<LogPageResponse>> GetAllAsync(
            string? search = null,
            string? level = null,
            string? method = null,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            int page = 1,
            int pageSize = 50,
            CancellationToken cancellationToken = default)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(search)) query.Add($"search={Uri.EscapeDataString(search)}");
            if (!string.IsNullOrWhiteSpace(level)) query.Add($"level={Uri.EscapeDataString(level)}");
            if (!string.IsNullOrWhiteSpace(method)) query.Add($"method={Uri.EscapeDataString(method)}");
            if (dateFrom is not null) query.Add($"dateFrom={dateFrom:yyyy-MM-dd}");
            if (dateTo is not null) query.Add($"dateTo={dateTo:yyyy-MM-dd}");
            query.Add($"page={page}");
            query.Add($"pageSize={pageSize}");

            var url = "api/Log" + (query.Count > 0 ? $"?{string.Join('&', query)}" : string.Empty);

            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                return await response.ToApiResultAsync<LogPageResponse>(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Il chiamante ha cancellato (es. nuova ricerca che sostituisce quella precedente):
                // non è un errore dell'utente, lascialo propagare.
                throw;
            }
            catch (Exception ex)
            {
                // Network error / DNS / TLS / CORS: arriva qui invece che a ToApiResultAsync
                // perché GetAsync lancia prima di avere una HttpResponseMessage.
                Console.Error.WriteLine($"Errore di rete GET {url}: {ex.Message}");
                return ApiResult<LogPageResponse>.WithError(
                    $"Errore di rete: {ex.Message}",
                    statusCode: 0);
            }
        }
    }
}
