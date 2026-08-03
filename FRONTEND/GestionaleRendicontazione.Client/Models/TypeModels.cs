namespace GestionaleRendicontazione.Client.Models
{
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
