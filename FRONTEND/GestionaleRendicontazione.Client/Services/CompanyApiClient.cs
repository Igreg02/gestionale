using System.Net.Http.Json;
using GestionaleRendicontazione.Client.Models;

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
}
