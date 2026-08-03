using System.Net.Http.Json;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Motore CRUD condiviso dai wrapper tipizzati (CompanyApiClient, ProjectApiClient,
    /// StatusApiClient, TypeApiClient, ...): stessi 5 metodi HTTP, cambia solo la route e
    /// i DTO. I wrapper restano perché ogni pagina Admin inietta un tipo concreto e i DTO
    /// (es. <see cref="CompanyResponse"/>) sono usati per nome nei razor — qui si accentra
    /// solo la logica di chiamata, non la superficie pubblica vista dalle pagine.
    /// </summary>
    public sealed class CrudApiClient<TResponse, TCreate, TUpdate>
    {
        private readonly HttpClient _http;
        private readonly string _route;

        public CrudApiClient(HttpClient http, string route)
        {
            _http = http;
            _route = route;
        }

        public async Task<List<TResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<List<TResponse>>(_route, ct);
            return result ?? [];
        }

        public async Task<TResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _http.GetFromJsonAsync<TResponse>($"{_route}/{id}", ct);

        public async Task<ApiResult<TResponse>> CreateAsync(TCreate dto, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync(_route, dto, ct);
            return await response.ToApiResultAsync<TResponse>(ct);
        }

        public async Task<ApiResult<TResponse>> UpdateAsync(Guid id, TUpdate dto, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"{_route}/{id}", dto, ct);
            return await response.ToApiResultAsync<TResponse>(ct);
        }

        public async Task<ApiResult> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"{_route}/{id}", ct);
            return await response.ToApiResultAsync(ct);
        }
    }
}
