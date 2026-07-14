using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    /// <summary>
    /// Aggrega i Worklog attivi (IsWorkLogDeleted = false) in un intervallo di date.
    /// Restituisce null quando l'FK radice (Project o Employee) non esiste.
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IDbContextService _dbContextService;

        public ReportService(IDbContextService dbContextService)
        {
            _dbContextService = dbContextService;
        }

        public Task<ProjectReportDto.Response?> GetProjectReportAsync(
            Guid projectId,
            DateOnly from,
            DateOnly to,
            CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var project = session.GetObjectByKey<Project>(projectId);
                if (project is null) return (ProjectReportDto.Response?)null;

                var worklogs = session.Query<WorkLog>()
                    .Where(w => !w.IsWorkLogDeleted
                                && w.Project != null
                                && w.Project.Id == projectId
                                && w.Date >= from
                                && w.Date <= to)
                    .OrderBy(w => w.Date)
                    .ToList();

                var responses = worklogs.Select(WorkLogMapper.ToAdminResponse).ToList();

                var byType = worklogs
                    .GroupBy(w => w.Type?.Name ?? string.Empty)
                    .Select(g => new ProjectReportDto.Bucket
                    {
                        Name = g.Key,
                        Hours = g.Sum(w => (decimal)w.HoursCounter),
                        Count = g.Count()
                    })
                    .OrderByDescending(b => b.Hours)
                    .ToList();

                var byStatus = worklogs
                    .GroupBy(w => w.Status?.Name ?? string.Empty)
                    .Select(g => new ProjectReportDto.Bucket
                    {
                        Name = g.Key,
                        Hours = g.Sum(w => (decimal)w.HoursCounter),
                        Count = g.Count()
                    })
                    .OrderByDescending(b => b.Hours)
                    .ToList();

                var byDay = worklogs
                    .GroupBy(w => w.Date)
                    .Select(g => new ProjectReportDto.DailyTotal
                    {
                        Date = g.Key,
                        Hours = g.Sum(w => (decimal)w.HoursCounter),
                        Count = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                var byEmployee = worklogs
                    .Where(w => w.Employee != null)
                    .GroupBy(w => w.Employee!.Oid)
                    .Select(g => new ProjectReportDto.EmployeeBucket
                    {
                        EmployeeId = g.Key,
                        UserName = g.First().Employee!.UserName ?? string.Empty,
                        FullName = BuildFullName(g.First().Employee!),
                        Hours = g.Sum(w => (decimal)w.HoursCounter),
                        Count = g.Count()
                    })
                    .OrderByDescending(e => e.Hours)
                    .ToList();

                return new ProjectReportDto.Response
                {
                    ProjectId = project.Id,
                    ProjectName = project.Name ?? string.Empty,
                    CompanyId = project.Company?.Id ?? Guid.Empty,
                    CompanyName = project.Company?.Name ?? string.Empty,
                    From = from,
                    To = to,
                    TotalHours = worklogs.Sum(w => (decimal)w.HoursCounter),
                    TotalWorkLogs = worklogs.Count,
                    UniqueEmployees = byEmployee.Count,
                    ByType = byType,
                    ByStatus = byStatus,
                    ByDay = byDay,
                    ByEmployee = byEmployee,
                    WorkLogs = responses
                };
            }));
        }

        public Task<EmployeeReportDto.Response?> GetEmployeeReportAsync(
            Guid employeeId,
            DateOnly from,
            DateOnly to,
            CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var employee = session.GetObjectByKey<Employee>(employeeId);
                if (employee is null) return (EmployeeReportDto.Response?)null;

                var worklogs = session.Query<WorkLog>()
                    .Where(w => !w.IsWorkLogDeleted
                                && w.Employee != null
                                && w.Employee.Oid == employeeId
                                && w.Date >= from
                                && w.Date <= to)
                    .OrderBy(w => w.Date)
                    .ToList();

                var responses = worklogs.Select(WorkLogMapper.ToAdminResponse).ToList();

                var byType = worklogs
                    .GroupBy(w => w.Type?.Name ?? string.Empty)
                    .Select(g => new EmployeeReportDto.Bucket
                    {
                        Name = g.Key,
                        Hours = g.Sum(w => (decimal)w.HoursCounter),
                        Count = g.Count()
                    })
                    .OrderByDescending(b => b.Hours)
                    .ToList();

                var byStatus = worklogs
                    .GroupBy(w => w.Status?.Name ?? string.Empty)
                    .Select(g => new EmployeeReportDto.Bucket
                    {
                        Name = g.Key,
                        Hours = g.Sum(w => (decimal)w.HoursCounter),
                        Count = g.Count()
                    })
                    .OrderByDescending(b => b.Hours)
                    .ToList();

                var byDay = worklogs
                    .GroupBy(w => w.Date)
                    .Select(g => new EmployeeReportDto.DailyTotal
                    {
                        Date = g.Key,
                        Hours = g.Sum(w => (decimal)w.HoursCounter),
                        Count = g.Count()
                    })
                    .OrderBy(d => d.Date)
                    .ToList();

                var byProject = worklogs
                    .Where(w => w.Project != null)
                    .GroupBy(w => w.Project!.Id)
                    .Select(g => new EmployeeReportDto.ProjectBucket
                    {
                        ProjectId = g.Key,
                        ProjectName = g.First().Project!.Name ?? string.Empty,
                        Hours = g.Sum(w => (decimal)w.HoursCounter),
                        Count = g.Count()
                    })
                    .OrderByDescending(p => p.Hours)
                    .ToList();

                return new EmployeeReportDto.Response
                {
                    EmployeeId = employee.Oid,
                    UserName = employee.UserName ?? string.Empty,
                    FullName = BuildFullName(employee),
                    From = from,
                    To = to,
                    TotalHours = worklogs.Sum(w => (decimal)w.HoursCounter),
                    TotalWorkLogs = worklogs.Count,
                    ByType = byType,
                    ByStatus = byStatus,
                    ByDay = byDay,
                    ByProject = byProject,
                    WorkLogs = responses
                };
            }));
        }

        private static string BuildFullName(Employee e)
        {
            var first = e.FirstName?.Trim() ?? string.Empty;
            var last = e.LastName?.Trim() ?? string.Empty;
            var full = $"{first} {last}".Trim();
            return string.IsNullOrEmpty(full) ? (e.UserName ?? string.Empty) : full;
        }
    }
}
