using System.Linq;
using GestionaleRendicontazione.Domain.Entities;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
    public static class WorkLogQueryExtensions
    {
        public static IQueryable<WorkLog> Active(this IQueryable<WorkLog> query)
        {
            return query.Where(w => !w.IsWorkLogDeleted);
        }
    }
}
