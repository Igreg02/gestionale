using System.Linq;
using GestionaleRendicontazione.Domain.Entities;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
    public static class WorkLogQueryExtensions
    {
        /// <summary>
        /// Filtra la query escludendo i WorkLog eliminati logicamente (soft delete).
        /// </summary>
        public static IQueryable<WorkLog> Active(this IQueryable<WorkLog> query)
        {
            return query.Where(w => !w.IsWorkLogDeleted);
        }
    }
}
