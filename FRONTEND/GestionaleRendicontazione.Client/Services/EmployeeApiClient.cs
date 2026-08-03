using System.Net.Http.Json;
using GestionaleRendicontazione.Client.Models;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Client tipizzato per l'endpoint <c>api/employee</c>.
    /// Solo Admin può accedere. Non esiste Create perché la creazione avviene
    /// tramite la registrazione (AuthController).
    /// </summary>
    public sealed class EmployeeApiClient
    {
        // Nessuna Create: la creazione avviene solo tramite AuthController.Register.
        // TCreate = object perché il motore condiviso la richiede comunque, ma il
        // metodo non viene esposto qui e quindi non è mai invocabile dall'esterno.
        private readonly HttpClient _http;
        private readonly CrudApiClient<EmployeeResponse, object, EmployeeUpdateRequest> _inner;

        public EmployeeApiClient(HttpClient http)
        {
            _http = http;
            _inner = new(http, "api/employee");
        }

        public Task<List<EmployeeResponse>> GetAllAsync(CancellationToken ct = default) => _inner.GetAllAsync(ct);

        public Task<EmployeeResponse?> GetByIdAsync(Guid id, CancellationToken ct = default) => _inner.GetByIdAsync(id, ct);

        public Task<ApiResult<EmployeeResponse>> UpdateAsync(Guid id, EmployeeUpdateRequest dto, CancellationToken ct = default)
            => _inner.UpdateAsync(id, dto, ct);

        public Task<ApiResult> DeleteAsync(Guid id, CancellationToken ct = default) => _inner.DeleteAsync(id, ct);

        public async Task<ApiResult> ForcePasswordResetAsync(Guid id, CancellationToken ct = default)
        {
            var response = await _http.PostAsync($"api/employee/{id}/force-password-reset", null, ct);
            return await response.ToApiResultAsync(ct);
        }
    }
}
