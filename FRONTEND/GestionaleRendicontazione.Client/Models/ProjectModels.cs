namespace GestionaleRendicontazione.Client.Models
{
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
