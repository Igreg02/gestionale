namespace GestionaleRendicontazione.Client.Models
{
    // ── DTO client-side (speculari a LogDto/LogPage del domain) ─────────────

    public sealed class LogPageResponse
    {
        public List<LogResponse> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }

    public sealed class LogResponse
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Livello { get; set; } = string.Empty;
        public string Messaggio { get; set; } = string.Empty;
        public string Metodo { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
}
