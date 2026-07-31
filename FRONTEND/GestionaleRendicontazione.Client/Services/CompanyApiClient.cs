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
        private readonly CrudApiClient<CompanyResponse, CompanyCreateRequest, CompanyUpdateRequest> _inner;

        public CompanyApiClient(HttpClient http) => _inner = new(http, "api/company");

        public Task<List<CompanyResponse>> GetAllAsync(CancellationToken ct = default) => _inner.GetAllAsync(ct);

        public Task<CompanyResponse?> GetByIdAsync(Guid id, CancellationToken ct = default) => _inner.GetByIdAsync(id, ct);

        public Task<ApiResult<CompanyResponse>> CreateAsync(CompanyCreateRequest dto, CancellationToken ct = default)
            => _inner.CreateAsync(dto, ct);

        public Task<ApiResult<CompanyResponse>> UpdateAsync(Guid id, CompanyUpdateRequest dto, CancellationToken ct = default)
            => _inner.UpdateAsync(id, dto, ct);

        public Task<ApiResult> DeleteAsync(Guid id, CancellationToken ct = default) => _inner.DeleteAsync(id, ct);
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
        public string Email { get; set; } = string.Empty;
    }

    public sealed class CompanyUpdateRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire il nome dell'azienda")]
        [System.ComponentModel.DataAnnotations.MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire l'email dell'azienda")]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
