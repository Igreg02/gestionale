using System.Net.Http.Json;

namespace GestionaleRendicontazione.Client.Services
{
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
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore di rete GET {url}: {ex.Message}");
                return ApiResult<LogPageResponse>.WithError(
                    $"Errore di rete: {ex.Message}",
                    statusCode: 0);
            }
        }
    }


    public sealed class LogPageResponse
    {
        public List<LogResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public sealed class LogResponse
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Livello { get; set; } = string.Empty;
        public string Messaggio { get; set; } = string.Empty;
        public string Metodo { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
