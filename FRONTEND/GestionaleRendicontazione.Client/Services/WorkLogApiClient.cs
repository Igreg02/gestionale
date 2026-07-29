using System.Net.Http.Json;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Wrapper su HttpClient per l'endpoint unico <c>api/worklog</c> (vedi Curl.md): il comportamento
    /// lato server dipende dal ruolo codificato nel token già allegato da
    /// <see cref="AuthenticatedHttpMessageHandler"/> — con un token User restituisce/filtra solo i
    /// worklog del dipendente autenticato, con un token Admin quelli di tutti (eventualmente filtrati).
    /// I metodi CRUD ritornano <see cref="ApiResult{T}"/> per esporre in modo tipizzato errori di
    /// validazione (400) e distinguere 401/403/404/5xx; i metodi di lookup tornano direttamente
    /// <see cref="List{T}"/> perché trattati come best-effort.
    /// </summary>
    public sealed class WorkLogApiClient
    {
        private readonly HttpClient _httpClient;

        public WorkLogApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResult<List<WorkLogResponseDto>>> GetAsync(
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

            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                return await response.ToApiResultAsync<List<WorkLogResponseDto>>(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Il chiamante ha cancellato: non è un errore dell'utente, lascialo propagare
                throw;
            }
            catch (Exception ex)
            {
                // Network error / DNS / TLS / CORS / body non-JSON: arriva qui invece che a
                // ToApiResultAsync perché GetAsync lancia prima di avere una HttpResponseMessage.
                Console.Error.WriteLine($"Errore di rete GET {url}: {ex.Message}");
                return ApiResult<List<WorkLogResponseDto>>.WithError(
                    $"Errore di rete: {ex.Message}",
                    statusCode: 0);
            }
        }

        public async Task<ApiResult<WorkLogResponseDto>> CreateAsync(
            WorkLogUpdateRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/worklog", dto, cancellationToken);
                return await response.ToApiResultAsync<WorkLogResponseDto>(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.Error.WriteLine($"Errore di rete POST api/worklog: {ex.Message}");
                return ApiResult<WorkLogResponseDto>.WithError(
                    $"Errore di rete: {ex.Message}",
                    statusCode: 0);
            }
        }

        public async Task<ApiResult<WorkLogResponseDto>> UpdateAsync(
            Guid id,
            WorkLogUpdateRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"api/worklog/{id}", dto, cancellationToken);
                return await response.ToApiResultAsync<WorkLogResponseDto>(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.Error.WriteLine($"Errore di rete PUT api/worklog/{id}: {ex.Message}");
                return ApiResult<WorkLogResponseDto>.WithError(
                    $"Errore di rete: {ex.Message}",
                    statusCode: 0);
            }
        }

        public async Task<ApiResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"api/worklog/{id}", cancellationToken);
                return await response.ToApiResultAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.Error.WriteLine($"Errore di rete DELETE api/worklog/{id}: {ex.Message}");
                return ApiResult.WithError($"Errore di rete: {ex.Message}", statusCode: 0);
            }
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

