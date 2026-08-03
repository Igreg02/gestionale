namespace GestionaleRendicontazione.Client.Models
{
    // ── DTO client-side ──────────────────────────────────────────────────────

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
