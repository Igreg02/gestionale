using System.Net.Http.Json;

namespace GestionaleRendicontazione.Client.Services
{
    public sealed class StatusApiClient
    {
        private readonly HttpClient _http;

        public StatusApiClient(HttpClient http) => _http = http;

        public async Task<List<StatusResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<List<StatusResponse>>("api/status", ct);
            return result ?? [];
        }

        public async Task<StatusResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _http.GetFromJsonAsync<StatusResponse>($"api/status/{id}", ct);

        public async Task<ApiResult<StatusResponse>> CreateAsync(
            StatusCreateRequest dto, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("api/status", dto, ct);
            return await response.ToApiResultAsync<StatusResponse>(ct);
        }

        public async Task<ApiResult<StatusResponse>> UpdateAsync(
            Guid id, StatusUpdateRequest dto, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"api/status/{id}", dto, ct);
            return await response.ToApiResultAsync<StatusResponse>(ct);
        }

        public async Task<ApiResult> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"api/status/{id}", ct);
            return await response.ToApiResultAsync(ct);
        }
    }


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
