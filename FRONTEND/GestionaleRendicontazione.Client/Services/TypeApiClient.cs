using System.Net.Http.Json;
using GestionaleRendicontazione.Client.Models;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Client tipizzato per l'endpoint <c>api/type</c>.
    /// GET è accessibile a tutti gli autenticati; scritture richiedono Admin.
    /// </summary>
    public sealed class TypeApiClient
    {
        private readonly CrudApiClient<WorkTypeResponse, WorkTypeCreateRequest, WorkTypeUpdateRequest> _inner;

        public TypeApiClient(HttpClient http) => _inner = new(http, "api/type");

        public Task<List<WorkTypeResponse>> GetAllAsync(CancellationToken ct = default) => _inner.GetAllAsync(ct);

        public Task<WorkTypeResponse?> GetByIdAsync(Guid id, CancellationToken ct = default) => _inner.GetByIdAsync(id, ct);

        public Task<ApiResult<WorkTypeResponse>> CreateAsync(WorkTypeCreateRequest dto, CancellationToken ct = default)
            => _inner.CreateAsync(dto, ct);

        public Task<ApiResult<WorkTypeResponse>> UpdateAsync(Guid id, WorkTypeUpdateRequest dto, CancellationToken ct = default)
            => _inner.UpdateAsync(id, dto, ct);

        public Task<ApiResult> DeleteAsync(Guid id, CancellationToken ct = default) => _inner.DeleteAsync(id, ct);
    }
}
