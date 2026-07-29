using System.Net.Http.Json;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Client per gli endpoint di report aggregato <c>api/report-data/project/{id}</c> e
    /// <c>api/report-data/employee/{id}</c> (vedi backend, Domain.Dtos.ReportDto). Entrambi richiedono
    /// il ruolo Admin e restituiscono, tra le altre cose, l'elenco worklog già filtrato lato server
    /// per il progetto/dipendente e il periodo richiesti — riusabile così com'è dal generatore PDF
    /// client-side (stessa forma di WorkLogResponseDto).
    /// </summary>
    public sealed class ReportDataApiClient
    {
        private readonly HttpClient _http;

        public ReportDataApiClient(HttpClient http) => _http = http;

        public async Task<ProjectReportResponseDto?> GetProjectReportAsync(
            Guid projectId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"api/report-data/project/{projectId}", from, to);
            var response = await _http.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ProjectReportResponseDto>(cancellationToken: cancellationToken);
        }

        public async Task<EmployeeReportResponseDto?> GetEmployeeReportAsync(
            Guid employeeId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"api/report-data/employee/{employeeId}", from, to);
            var response = await _http.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<EmployeeReportResponseDto>(cancellationToken: cancellationToken);
        }

        private static string BuildUrl(string basePath, DateOnly? from, DateOnly? to)
        {
            var query = new List<string>();
            if (from is not null) query.Add($"from={from:yyyy-MM-dd}");
            if (to is not null) query.Add($"to={to:yyyy-MM-dd}");
            return query.Count > 0 ? $"{basePath}?{string.Join('&', query)}" : basePath;
        }
    }

    // ── DTO client-side (sottoinsieme di Domain.Dtos.ReportDto utile al PDF) ──

    public sealed class ProjectReportResponseDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public int TotalWorkLogs { get; set; }
        public int UniqueEmployees { get; set; }
        public List<WorkLogResponseDto> WorkLogs { get; set; } = new();
    }

    public sealed class EmployeeReportResponseDto
    {
        public Guid EmployeeId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public int TotalWorkLogs { get; set; }
        public List<WorkLogResponseDto> WorkLogs { get; set; } = new();
    }
}
