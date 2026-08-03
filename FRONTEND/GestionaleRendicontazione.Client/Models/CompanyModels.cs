namespace GestionaleRendicontazione.Client.Models
{
    // ── DTO client-side (speculari a CompanyDto del domain) ─────────────────

    public sealed class CompanyResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }

    public sealed class CompanyCreateRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire il nome dell'azienda")]
        [System.ComponentModel.DataAnnotations.MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire l'email dell'azienda")]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public sealed class CompanyUpdateRequest
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire il nome dell'azienda")]
        [System.ComponentModel.DataAnnotations.MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Devi inserire l'email dell'azienda")]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}
