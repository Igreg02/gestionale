using System.Net.Http.Json;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Client tipizzato per l'endpoint <c>api/type</c>.
    /// GET è accessibile a tutti gli autenticati; scritture richiedono Admin.
    /// </summary>
    public sealed class TypeApiClient
    {
        private readonly HttpClient _http;

        public TypeApiClient(HttpClient http) => _http = http;

        public async Task<List<WorkTypeResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<List<WorkTypeResponse>>("api/type", ct);
            return result ?? [];
        }

        public async Task<WorkTypeResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _http.GetFromJsonAsync<WorkTypeResponse>($"api/type/{id}", ct);

        public async Task<ApiResult<WorkTypeResponse>> CreateAsync(
            WorkTypeCreateRequest dto, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("api/type", dto, ct);
            return await response.ToApiResultAsync<WorkTypeResponse>(ct);
        }

        public async Task<ApiResult<WorkTypeResponse>> UpdateAsync(
            Guid id, WorkTypeUpdateRequest dto, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"api/type/{id}", dto, ct);
            return await response.ToApiResultAsync<WorkTypeResponse>(ct);
        }

        public async Task<ApiResult> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"api/type/{id}", ct);
            return await response.ToApiResultAsync(ct);
        }
    }

    // ── DTO client-side ──────────────────────────────────────────────────────

    public sealed class WorkTypeResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class WorkTypeCreateRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "La tipologia deve avere un nome")]
        public string Name { get; set; } = string.Empty;
    }

    public sealed class WorkTypeUpdateRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "La tipologia deve avere un nome")]
        public string Name { get; set; } = string.Empty;
    }
}
