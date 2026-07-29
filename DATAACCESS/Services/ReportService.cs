using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Dataaccess.Helpers;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class ReportService : IReportService
    {
        private readonly IDbContextService _dbContextService;
        private readonly IMapper _mapper;

        public ReportService(IDbContextService dbContextService, IMapper mapper)
        {
            _dbContextService = dbContextService;
            _mapper = mapper;
        }

        public Task<ReportDto.Project.Response?> GetProjectReportAsync(
            Guid projectId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var project = session.GetObjectByKey<Project>(projectId);
                if (project is null) return (ReportDto.Project.Response?)null;

                var worklogs = GetWorklogsInRange(session, w => w.Project != null && w.Project.Id == projectId, from, to);
                var responses = _mapper.Map<List<WorkLogDto.Admin.Response>>(worklogs);

                var byType = AggregateByKey(worklogs, w => w.Type?.Name ?? string.Empty)
                    .Select(x => new ReportDto.Bucket { Name = x.Key, Hours = x.Hours, Count = x.Count })
                    .ToList();

                var byStatus = AggregateByKey(worklogs, w => w.Status?.Name ?? string.Empty)
                    .Select(x => new ReportDto.Bucket { Name = x.Key, Hours = x.Hours, Count = x.Count })
                    .ToList();

                var byDay = AggregateByDay(worklogs)
                    .Select(x => new ReportDto.DailyTotal { Date = x.Date, Hours = x.Hours, Count = x.Count })
                    .ToList();

                var byEmployee = worklogs
                    .Where(w => w.Employee != null)
                    .GroupBy(w => w.Employee!.Id)
                    .Select(g => new ReportDto.Project.EmployeeBucket
                    {
                        EmployeeId = g.Key,
                        UserName = g.First().Employee!.UserName ?? string.Empty,
                        FullName = BuildFullName(g.First().Employee!),
                        Hours = g.Sum(w => (decimal)w.HoursCounter),
                        Count = g.Count()
                    })
                    .OrderByDescending(e => e.Hours)
                    .ToList();

                return new ReportDto.Project.Response
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

        public Task<ReportDto.Employee.Response?> GetEmployeeReportAsync(
            Guid employeeId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var employee = session.GetObjectByKey<Employee>(employeeId);
                if (employee is null) return (ReportDto.Employee.Response?)null;

                var worklogs = GetWorklogsInRange(session, w => w.Employee != null && w.Employee.Id == employeeId, from, to);
                var responses = _mapper.Map<List<WorkLogDto.Admin.Response>>(worklogs);

                var byType = AggregateByKey(worklogs, w => w.Type?.Name ?? string.Empty)
                    .Select(x => new ReportDto.Bucket { Name = x.Key, Hours = x.Hours, Count = x.Count })
                    .ToList();

                var byStatus = AggregateByKey(worklogs, w => w.Status?.Name ?? string.Empty)
                    .Select(x => new ReportDto.Bucket { Name = x.Key, Hours = x.Hours, Count = x.Count })
                    .ToList();

                var byDay = AggregateByDay(worklogs)
                    .Select(x => new ReportDto.DailyTotal { Date = x.Date, Hours = x.Hours, Count = x.Count })
                    .ToList();

                var byProject = worklogs
                    .Where(w => w.Project != null)
                    .GroupBy(w => w.Project!.Id)
                    .Select(g => new ReportDto.Employee.ProjectBucket
                    {
                        ProjectId = g.Key,
                        ProjectName = g.First().Project!.Name ?? string.Empty,
                        Hours = g.Sum(w => (decimal)w.HoursCounter),
                        Count = g.Count()
                    })
                    .OrderByDescending(p => p.Hours)
                    .ToList();

                return new ReportDto.Employee.Response
                {
                    EmployeeId = employee.Id,
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

        private static List<WorkLog> GetWorklogsInRange(
            Session session,
            System.Linq.Expressions.Expression<Func<WorkLog, bool>> rootFilter,
            DateOnly? from,
            DateOnly? to)
        {
            var query = session.Query<WorkLog>()
                .Active()
                .Where(rootFilter);

            if (from.HasValue)
            {
                var fromValue = from.Value;
                query = query.Where(w => w.Date >= fromValue);
            }
            if (to.HasValue)
            {
                var toValue = to.Value;
                query = query.Where(w => w.Date <= toValue);
            }

            return query
                .OrderBy(w => w.Date)
                .ToList();
        }

        private static List<(string Key, decimal Hours, int Count)> AggregateByKey(
            List<WorkLog> worklogs,
            Func<WorkLog, string> keySelector)
        {
            return worklogs
                .GroupBy(keySelector)
                .Select(g => (Key: g.Key, Hours: g.Sum(w => (decimal)w.HoursCounter), Count: g.Count()))
                .OrderByDescending(x => x.Hours)
                .ToList();
        }

        private static List<(DateOnly Date, decimal Hours, int Count)> AggregateByDay(List<WorkLog> worklogs)
        {
            return worklogs
                .GroupBy(w => w.Date)
                .Select(g => (Date: g.Key, Hours: g.Sum(w => (decimal)w.HoursCounter), Count: g.Count()))
                .OrderBy(x => x.Date)
                .ToList();
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
