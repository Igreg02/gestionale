using System.Net.Http.Json;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Client tipizzato per l'endpoint <c>api/status</c>.
    /// GET è accessibile a tutti gli autenticati; scritture richiedono Admin.
    /// </summary>
    public sealed class StatusApiClient
    {
        private readonly CrudApiClient<StatusResponse, StatusCreateRequest, StatusUpdateRequest> _inner;

        public StatusApiClient(HttpClient http) => _inner = new(http, "api/status");

        public Task<List<StatusResponse>> GetAllAsync(CancellationToken ct = default) => _inner.GetAllAsync(ct);

        public Task<StatusResponse?> GetByIdAsync(Guid id, CancellationToken ct = default) => _inner.GetByIdAsync(id, ct);

        public Task<ApiResult<StatusResponse>> CreateAsync(StatusCreateRequest dto, CancellationToken ct = default)
            => _inner.CreateAsync(dto, ct);

        public Task<ApiResult<StatusResponse>> UpdateAsync(Guid id, StatusUpdateRequest dto, CancellationToken ct = default)
            => _inner.UpdateAsync(id, dto, ct);

        public Task<ApiResult> DeleteAsync(Guid id, CancellationToken ct = default) => _inner.DeleteAsync(id, ct);
    }

    // ── DTO client-side ──────────────────────────────────────────────────────

    public sealed class StatusResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class StatusCreateRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Lo stato deve avere un nome")]
        public string Name { get; set; } = string.Empty;
    }

    public sealed class StatusUpdateRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Lo stato deve avere un nome")]
        public string Name { get; set; } = string.Empty;
    }
}
