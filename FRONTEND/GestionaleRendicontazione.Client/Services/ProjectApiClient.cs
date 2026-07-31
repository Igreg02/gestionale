using System.Net.Http.Json;

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

    // ── DTO client-side ──────────────────────────────────────────────────────

    public sealed class ProjectResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid IdCompany { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    }

    public sealed class ProjectCreateRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Il nome del progetto è obbligatorio.")]
        [System.ComponentModel.DataAnnotations.MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "L'azienda di appartenenza è obbligatoria.")]
        public Guid IdCompany { get; set; }
    }

    public sealed class ProjectUpdateRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Il nome del progetto è obbligatorio.")]
        [System.ComponentModel.DataAnnotations.MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "L'azienda di appartenenza è obbligatoria.")]
        public Guid IdCompany { get; set; }
    }
}
