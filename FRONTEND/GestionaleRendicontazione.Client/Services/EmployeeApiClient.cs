using System.Net.Http.Json;

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
        private readonly CrudApiClient<EmployeeResponse, object, EmployeeUpdateRequest> _inner;

        public EmployeeApiClient(HttpClient http) => _inner = new(http, "api/employee");

        public Task<List<EmployeeResponse>> GetAllAsync(CancellationToken ct = default) => _inner.GetAllAsync(ct);

        public Task<EmployeeResponse?> GetByIdAsync(Guid id, CancellationToken ct = default) => _inner.GetByIdAsync(id, ct);

        public Task<ApiResult<EmployeeResponse>> UpdateAsync(Guid id, EmployeeUpdateRequest dto, CancellationToken ct = default)
            => _inner.UpdateAsync(id, dto, ct);

        public Task<ApiResult> DeleteAsync(Guid id, CancellationToken ct = default) => _inner.DeleteAsync(id, ct);
    }

    // ── DTO client-side ──────────────────────────────────────────────────────

    public sealed class EmployeeResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
    }

    public sealed class EmployeeUpdateRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire un username")]
        [System.ComponentModel.DataAnnotations.MaxLength(255)]
        public string Username { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire il nome dell'utente")]
        [System.ComponentModel.DataAnnotations.MaxLength(255)]
        public string FirstName { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire il cognome dell'utente")]
        [System.ComponentModel.DataAnnotations.MaxLength(255)]
        public string LastName { get; set; } = string.Empty;
    }
}
