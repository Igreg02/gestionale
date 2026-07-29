using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    /// <summary>
    /// Servizio di sola lettura per i log applicativi. I log sono scritti
    /// esclusivamente dal sink Serilog lato server; qui si espone solo la
    /// query con filtri per la pagina admin di Logs.
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// Restituisce i log applicativi ordinati per data decrescente,
        /// applicando i filtri passati. Tutti i parametri sono opzionali:
        /// un valore null/vuoto significa "filtro non applicato".
        /// </summary>
        /// <param name="search">Testo libero: cerca (case-insensitive) in Messaggio, Path e UserId.</param>
        /// <param name="level">Livello di log esatto (es. "Information", "Warning", "Error").</param>
        /// <param name="method">Metodo HTTP (GET, POST, ...) — match esatto.</param>
        /// <param name="dateFrom">Data/ora minima (inclusiva) del log.</param>
        /// <param name="dateTo">Data/ora massima (inclusiva) del log.</param>
        /// <param name="page">Pagina 1-based.</param>
        /// <param name="pageSize">Numero di elementi per pagina (default 50, max 500).</param>
        Task<LogPage> GetAllAsync(
            string? search = null,
            string? level = null,
            string? method = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 50,
            CancellationToken ct = default);
    }

    /// <summary>
    /// Risultato paginato per la lista dei log. <see cref="TotalCount"/> è
    /// il totale dei record che soddisfano i filtri, prima della paginazione.
    /// </summary>
    public class LogPage
    {
        public List<LogDto.Response> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
