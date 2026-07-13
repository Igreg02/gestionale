namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Aggregazione rendicontazioni per dipendente/periodo.
    /// Il frontend compone il PDF a partire da questa struttura.
    /// </summary>
    public class EmployeeReportDto
    {
        public class Response
        {
            public Guid EmployeeId { get; set; }
            public string UserName { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;

            public DateOnly From { get; set; }
            public DateOnly To { get; set; }

            public decimal TotalHours { get; set; }
            public int TotalWorkLogs { get; set; }

            public List<Bucket> ByType { get; set; } = new();
            public List<Bucket> ByStatus { get; set; } = new();
            public List<DailyTotal> ByDay { get; set; } = new();
            public List<ProjectBucket> ByProject { get; set; } = new();

            public List<WorkLogAdminDto.Response> WorkLogs { get; set; } = new();
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

        public class ProjectBucket
        {
            public Guid ProjectId { get; set; }
            public string ProjectName { get; set; } = string.Empty;
            public decimal Hours { get; set; }
            public int Count { get; set; }
        }
    }
}
