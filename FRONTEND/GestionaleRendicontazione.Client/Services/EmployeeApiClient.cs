using System.Net.Http.Json;

namespace GestionaleRendicontazione.Client.Services
{
    public sealed class EmployeeApiClient
    {
        private readonly HttpClient _http;

        public EmployeeApiClient(HttpClient http) => _http = http;

        public async Task<List<EmployeeResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<List<EmployeeResponse>>("api/employee", ct);
            return result ?? [];
        }

        public async Task<EmployeeResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _http.GetFromJsonAsync<EmployeeResponse>($"api/employee/{id}", ct);

        public async Task<ApiResult<EmployeeResponse>> UpdateAsync(
            Guid id, EmployeeUpdateRequest dto, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"api/employee/{id}", dto, ct);
            return await response.ToApiResultAsync<EmployeeResponse>(ct);
        }

        public async Task<ApiResult> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"api/employee/{id}", ct);
            return await response.ToApiResultAsync(ct);
        }
    }


    public sealed class EmployeeResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public sealed class EmployeeUpdateRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire un username")]
        [System.ComponentModel.DataAnnotations.MaxLength(255)]
        public string Username { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire il nome dell'utente")]
        [System.ComponentModel.DataAnnotations.MaxLength(255)]
        public string FirstName { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire il cognome dell'utente")]
        [System.ComponentModel.DataAnnotations.MaxLength(255)]
        public string LastName { get; set; } = string.Empty;
    }
}
