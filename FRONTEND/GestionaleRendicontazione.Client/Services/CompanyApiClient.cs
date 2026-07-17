using System.Net.Http.Json;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Client tipizzato per l'endpoint <c>api/company</c>.
    /// Tutti i metodi di scrittura richiedono ruolo Admin (il token viene allegato
    /// automaticamente da <see cref="AuthenticatedHttpMessageHandler"/>).
    /// </summary>
    public sealed class CompanyApiClient
    {
        private readonly HttpClient _http;

        public CompanyApiClient(HttpClient http) => _http = http;

        public async Task<List<CompanyResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<List<CompanyResponse>>("api/company", ct);
            return result ?? [];
        }

        public async Task<CompanyResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _http.GetFromJsonAsync<CompanyResponse>($"api/company/{id}", ct);

        public async Task<ApiResult<CompanyResponse>> CreateAsync(
            CompanyCreateRequest dto, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("api/company", dto, ct);
            return await response.ToApiResultAsync<CompanyResponse>(ct);
        }

        public async Task<ApiResult<CompanyResponse>> UpdateAsync(
            Guid id, CompanyUpdateRequest dto, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"api/company/{id}", dto, ct);
            return await response.ToApiResultAsync<CompanyResponse>(ct);
        }

        public async Task<ApiResult> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"api/company/{id}", ct);
            return await response.ToApiResultAsync(ct);
        }
    }

    // ── DTO client-side (speculari a CompanyDto del domain) ─────────────────

    public sealed class CompanyResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public sealed class CompanyCreateRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire il nome dell'azienda")]
        [System.ComponentModel.DataAnnotations.MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire l'email dell'azienda")]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string email { get; set; } = string.Empty;
    }

    public sealed class CompanyUpdateRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire il nome dell'azienda")]
        [System.ComponentModel.DataAnnotations.MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire l'email dell'azienda")]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string email { get; set; } = string.Empty;
    }
}
