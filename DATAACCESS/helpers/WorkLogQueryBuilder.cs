using System;
using System.Linq;
using GestionaleRendicontazione.Domain.Entities;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
    /// <summary>
    /// Catena di filtri opzionali applicabili a una query <see cref="WorkLog"/>.
    /// Estratto perché WorkLogAdminService e WorkLogUserService applicano lo stesso
    /// insieme di filtri (employeeId, projectId, dateFrom, dateTo, statusName).
    /// </summary>
    public static class WorkLogQueryBuilder
    {
        public static IQueryable<WorkLog> ApplyFilters(
            this IQueryable<WorkLog> query,
            Guid? employeeId = null,
            Guid? projectId = null,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            string? statusName = null)
        {
            if (employeeId.HasValue)
            {
                query = query.Where(w => w.Employee != null && w.Employee.Id == employeeId.Value);
            }
            if (projectId.HasValue)
            {
                query = query.Where(w => w.Project != null && w.Project.Id == projectId.Value);
            }
            if (dateFrom.HasValue)
            {
                var from = dateFrom.Value;
                query = query.Where(w => w.Date >= from);
            }
            if (dateTo.HasValue)
            {
                var to = dateTo.Value;
                query = query.Where(w => w.Date <= to);
            }
            if (!string.IsNullOrWhiteSpace(statusName))
            {
                var trimmed = statusName.Trim();
                query = query.Where(w => w.Status != null && w.Status.Name == trimmed);
            }
            return query;
        }
    }
}