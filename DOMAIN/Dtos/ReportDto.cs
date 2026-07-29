namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Contenitore dei DTO relativi alla reportistica (per progetto e per dipendente).
    /// Server -> Client
    /// </summary>
    public class ReportDto
    {
        /// <summary>Tipi comuni condivisi da Project.Response ed Employee.Response.</summary>
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

        /// <summary>
        /// Aggregazione rendicontazioni per progetto/periodo.
        /// Include il worklog completo di tutti i dipendenti coinvolti.
        /// </summary>
        public class Project
        {
            public class Response
            {
                public Guid ProjectId { get; set; }
                public string ProjectName { get; set; } = string.Empty;
                public Guid CompanyId { get; set; }
                public string CompanyName { get; set; } = string.Empty;

                public DateOnly? From { get; set; }
                public DateOnly? To { get; set; }

                public decimal TotalHours { get; set; }
                public int TotalWorkLogs { get; set; }
                public int UniqueEmployees { get; set; }

                public List<Bucket> ByType { get; set; } = new();
                public List<Bucket> ByStatus { get; set; } = new();
                public List<DailyTotal> ByDay { get; set; } = new();
                public List<EmployeeBucket> ByEmployee { get; set; } = new();

                public List<WorkLogDto.Admin.Response> WorkLogs { get; set; } = new();
            }

            /// <summary>Specifico del report per progetto: aggregazione per dipendente coinvolto.</summary>
            public class EmployeeBucket
            {
                public Guid EmployeeId { get; set; }
                public string UserName { get; set; } = string.Empty;
                public string FullName { get; set; } = string.Empty;
                public decimal Hours { get; set; }
                public int Count { get; set; }
            }
        }

        /// <summary>
        /// Aggregazione rendicontazioni per dipendente/periodo.
        /// Il frontend compone il PDF a partire da questa struttura.
        /// </summary>
        public class Employee
        {
            public class Response
            {
                public Guid EmployeeId { get; set; }
                public string UserName { get; set; } = string.Empty;
                public string FullName { get; set; } = string.Empty;

                public DateOnly? From { get; set; }
                public DateOnly? To { get; set; }

                public decimal TotalHours { get; set; }
                public int TotalWorkLogs { get; set; }

                public List<Bucket> ByType { get; set; } = new();
                public List<Bucket> ByStatus { get; set; } = new();
                public List<DailyTotal> ByDay { get; set; } = new();
                public List<ProjectBucket> ByProject { get; set; } = new();

                public List<WorkLogDto.Admin.Response> WorkLogs { get; set; } = new();
            }

            /// <summary>Specifico del report per dipendente: aggregazione per progetto rendicontato.</summary>
            public class ProjectBucket
            {
                public Guid ProjectId { get; set; }
                public string ProjectName { get; set; } = string.Empty;
                public decimal Hours { get; set; }
                public int Count { get; set; }
            }
        }
    }
}
