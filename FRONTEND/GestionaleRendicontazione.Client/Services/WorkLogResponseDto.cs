namespace GestionaleRendicontazione.Client.Services
{
    public sealed class WorkLogResponseDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public float HoursCounter { get; set; }
        public DateOnly Date { get; set; }
        public DateTime? CreateAt { get; set; }
        public DateTime? UpdateAt { get; set; }

        public Guid IdProject { get; set; }
        public string ProjectName { get; set; } = string.Empty;

        public Guid? IdEmployee { get; set; }
        public string? EmployeeName { get; set; }

        public Guid IdType { get; set; }
        public string TypeName { get; set; } = string.Empty;

        public Guid IdStatus { get; set; }
        public string StatusName { get; set; } = string.Empty;
    }
}
