using System.Net.Http.Json;
using GestionaleRendicontazione.Client.Models;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Client per l'endpoint di report aggregato <c>api/report-data/project/{id}</c>
    /// (vedi backend, Domain.Dtos.ReportDto). Richiede il ruolo Admin e restituisce, tra le altre cose,
    /// l'elenco worklog già filtrato lato server per il progetto e il periodo richiesti — riusabile così
    /// com'è dal generatore PDF client-side (stessa forma di WorkLogResponseDto).
    /// </summary>
    public sealed class ReportDataApiClient
    {
        private readonly HttpClient _http;

        public ReportDataApiClient(HttpClient http) => _http = http;

        public async Task<ApiResult<ProjectReportResponseDto>> GetProjectReportAsync(
            Guid projectId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"api/report-data/project/{projectId}", from, to);
            try
            {
                var response = await _http.GetAsync(url, cancellationToken);
                return await response.ToApiResultAsync<ProjectReportResponseDto>(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // Network error / DNS / TLS: arriva qui invece che a ToApiResultAsync
                // perché GetAsync lancia prima di avere una HttpResponseMessage.
                Console.Error.WriteLine($"Errore di rete GET {url}: {ex.Message}");
                return ApiResult<ProjectReportResponseDto>.WithError(
                    $"Errore di rete: {ex.Message}",
                    statusCode: 0);
            }
        }

        private static string BuildUrl(string basePath, DateOnly? from, DateOnly? to)
        {
            var query = new List<string>();
            if (from is not null) query.Add($"from={from:yyyy-MM-dd}");
            if (to is not null) query.Add($"to={to:yyyy-MM-dd}");
            return query.Count > 0 ? $"{basePath}?{string.Join('&', query)}" : basePath;
        }
    }
}
