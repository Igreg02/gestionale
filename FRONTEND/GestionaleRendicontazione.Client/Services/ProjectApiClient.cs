using System.Net.Http.Json;

namespace GestionaleRendicontazione.Client.Services
{
    public sealed class ProjectApiClient
    {
        private readonly HttpClient _http;

        public ProjectApiClient(HttpClient http) => _http = http;

        public async Task<List<ProjectResponse>> GetAllAsync(CancellationToken ct = default)
        {
            var result = await _http.GetFromJsonAsync<List<ProjectResponse>>("api/project", ct);
            return result ?? [];
        }

        public async Task<ProjectResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
            => await _http.GetFromJsonAsync<ProjectResponse>($"api/project/{id}", ct);

        public async Task<ApiResult<ProjectResponse>> CreateAsync(
            ProjectCreateRequest dto, CancellationToken ct = default)
        {
            var response = await _http.PostAsJsonAsync("api/project", dto, ct);
            return await response.ToApiResultAsync<ProjectResponse>(ct);
        }

        public async Task<ApiResult<ProjectResponse>> UpdateAsync(
            Guid id, ProjectUpdateRequest dto, CancellationToken ct = default)
        {
            var response = await _http.PutAsJsonAsync($"api/project/{id}", dto, ct);
            return await response.ToApiResultAsync<ProjectResponse>(ct);
        }

        public async Task<ApiResult> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.DeleteAsync($"api/project/{id}", ct);
            return await response.ToApiResultAsync(ct);
        }
    }


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
