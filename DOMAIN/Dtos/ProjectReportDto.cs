namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Aggregazione rendicontazioni per progetto/periodo.
    /// Include il worklog completo di tutti i dipendenti coinvolti.
    /// </summary>
    public class ProjectReportDto
    {
        public class Response
        {
            public Guid ProjectId { get; set; }
            public string ProjectName { get; set; } = string.Empty;
            public Guid CompanyId { get; set; }
            public string CompanyName { get; set; } = string.Empty;

            public DateOnly From { get; set; }
            public DateOnly To { get; set; }

            public decimal TotalHours { get; set; }
            public int TotalWorkLogs { get; set; }
            public int UniqueEmployees { get; set; }

            public List<Bucket> ByType { get; set; } = new();
            public List<Bucket> ByStatus { get; set; } = new();
            public List<DailyTotal> ByDay { get; set; } = new();
            public List<EmployeeBucket> ByEmployee { get; set; } = new();

            public List<WorkLogDto.Admin.Response> WorkLogs { get; set; } = new();
        }

        public class Bucket
        {
            public string Name { get; set; } = string.Empty;
            public decimal Hours { get; set; }
            public int Count { get; set; }
        }

        public class DailyTotal
        {
            public DateOnly Date { get; set; }
            public decimal Hours { get; set; }
            public int Count { get; set; }
        }

        public class EmployeeBucket
        {
            public Guid EmployeeId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
            public decimal Hours { get; set; }
            public int Count { get; set; }
        }
    }
}
