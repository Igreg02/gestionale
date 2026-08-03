namespace GestionaleRendicontazione.Client.Models
{
    // ── DTO client-side ──────────────────────────────────────────────────────

    public sealed class EmployeeResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}".Trim();
        public bool MustChangePassword { get; set; }
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
