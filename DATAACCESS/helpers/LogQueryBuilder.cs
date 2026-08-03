using System;
using System.Linq;
using GestionaleRendicontazione.Domain.Entities;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
    /// <summary>
    /// Filtri opzionali e paginazione per la query dei log applicativi.
    /// Estratto da LogService.GetAllAsync per separare "filtri" da "paginazione" da "mapping".
    /// </summary>
    public static class LogQueryBuilder
    {
        public const int DefaultPageSize = 50;
        public const int MaxPageSize = 500;

        public static IQueryable<LogApplicativo> ApplyFilters(
            this IQueryable<LogApplicativo> query,
            string? search = null,
            string? level = null,
            string? method = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(l =>
                    l.Messaggio.Contains(term) ||
                    l.Path.Contains(term) ||
                    l.UserId.Contains(term));
            }
            if (!string.IsNullOrWhiteSpace(level))
            {
                var lv = level.Trim();
                query = query.Where(l => l.Livello == lv);
            }
            if (!string.IsNullOrWhiteSpace(method))
            {
                var m = method.Trim();
                query = query.Where(l => l.Metodo == m);
            }
            if (dateFrom.HasValue)
            {
                var from = dateFrom.Value;
                query = query.Where(l => l.Data >= from);
            }
            if (dateTo.HasValue)
            {
                // dateTo arriva come data pura (es. "2026-07-31" -> 00:00:00): un confronto <= lo
                // tratterebbe come "fino a mezzanotte", escludendo tutti i log dello stesso giorno
                // scritti dopo le 00:00. Confrontiamo invece con l'inizio del giorno successivo.
                var toExclusive = dateTo.Value.Date.AddDays(1);
                query = query.Where(l => l.Data < toExclusive);
            }
            return query;
        }

        public static (int page, int pageSize) NormalizePaging(int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize <= 0) pageSize = DefaultPageSize;
            if (pageSize > MaxPageSize) pageSize = MaxPageSize;
            return (page, pageSize);
        }
    }
}