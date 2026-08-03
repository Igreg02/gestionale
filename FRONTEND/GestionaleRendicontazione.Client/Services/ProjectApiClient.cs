using System.Net.Http.Json;
using GestionaleRendicontazione.Client.Models;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Client tipizzato per l'endpoint <c>api/project</c>.
    /// GET è accessibile a tutti gli autenticati; POST/PUT/DELETE richiedono Admin.
    /// </summary>
    public sealed class ProjectApiClient
    {
        private readonly CrudApiClient<ProjectResponse, ProjectCreateRequest, ProjectUpdateRequest> _inner;

        public ProjectApiClient(HttpClient http) => _inner = new(http, "api/project");

        public Task<List<ProjectResponse>> GetAllAsync(CancellationToken ct = default) => _inner.GetAllAsync(ct);

        public Task<ProjectResponse?> GetByIdAsync(Guid id, CancellationToken ct = default) => _inner.GetByIdAsync(id, ct);

        public Task<ApiResult<ProjectResponse>> CreateAsync(ProjectCreateRequest dto, CancellationToken ct = default)
            => _inner.CreateAsync(dto, ct);

        public Task<ApiResult<ProjectResponse>> UpdateAsync(Guid id, ProjectUpdateRequest dto, CancellationToken ct = default)
            => _inner.UpdateAsync(id, dto, ct);

        public Task<ApiResult> DeleteAsync(Guid id, CancellationToken ct = default) => _inner.DeleteAsync(id, ct);
    }
}
