using System.Net.Http.Json;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Wrapper su HttpClient per l'endpoint unico <c>api/worklog</c> (vedi Curl.md): il comportamento
    /// lato server dipende dal ruolo codificato nel token già allegato da
    /// <see cref="AuthenticatedHttpMessageHandler"/> — con un token User restituisce/filtra solo i
    /// worklog del dipendente autenticato, con un token Admin quelli di tutti (eventualmente filtrati).
    /// </summary>
    public sealed class WorkLogApiClient
    {
        private readonly HttpClient _httpClient;

        public WorkLogApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<WorkLogResponseDto>> GetAsync(
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            Guid? employeeId = null,
            Guid? projectId = null,
            string? statusName = null,
            CancellationToken cancellationToken = default)
        {
            var query = new List<string>();
            if (dateFrom is not null) query.Add($"dateFrom={dateFrom:yyyy-MM-dd}");
            if (dateTo is not null) query.Add($"dateTo={dateTo:yyyy-MM-dd}");
            if (employeeId is not null) query.Add($"employeeId={employeeId}");
            if (projectId is not null) query.Add($"projectId={projectId}");
            if (!string.IsNullOrWhiteSpace(statusName)) query.Add($"statusName={Uri.EscapeDataString(statusName)}");

            var url = "api/worklog" + (query.Count > 0 ? $"?{string.Join('&', query)}" : string.Empty);

            var result = await _httpClient.GetFromJsonAsync<List<WorkLogResponseDto>>(url, cancellationToken);
            return result ?? new List<WorkLogResponseDto>();
        }

        public async Task<WorkLogResponseDto?> CreateAsync(
            WorkLogUpdateRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("api/worklog", dto, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WorkLogResponseDto>(cancellationToken: cancellationToken);
            }
            return null;
        }

        public async Task<WorkLogResponseDto?> UpdateAsync(
            Guid id,
            WorkLogUpdateRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/worklog/{id}", dto, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<WorkLogResponseDto>(cancellationToken: cancellationToken);
            }
            return null;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.DeleteAsync($"api/worklog/{id}", cancellationToken);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ProjectResponseDto>> GetProjectsAsync(CancellationToken cancellationToken = default)
        {
            var result = await _httpClient.GetFromJsonAsync<List<ProjectResponseDto>>("api/project", cancellationToken);
            return result ?? new List<ProjectResponseDto>();
        }

        public async Task<List<StatusResponseDto>> GetStatusesAsync(CancellationToken cancellationToken = default)
        {
            var result = await _httpClient.GetFromJsonAsync<List<StatusResponseDto>>("api/status", cancellationToken);
            return result ?? new List<StatusResponseDto>();
        }

        public async Task<List<TypeResponseDto>> GetTypesAsync(CancellationToken cancellationToken = default)
        {
            var result = await _httpClient.GetFromJsonAsync<List<TypeResponseDto>>("api/type", cancellationToken);
            return result ?? new List<TypeResponseDto>();
        }

        public async Task<List<EmployeeResponseDto>> GetEmployeesAsync(CancellationToken cancellationToken = default)
        {
            var result = await _httpClient.GetFromJsonAsync<List<EmployeeResponseDto>>("api/employee", cancellationToken);
            return result ?? new List<EmployeeResponseDto>();
        }
    }
}

