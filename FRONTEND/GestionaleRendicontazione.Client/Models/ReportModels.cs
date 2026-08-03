namespace GestionaleRendicontazione.Client.Models
{
    // ── DTO client-side (sottoinsieme di Domain.Dtos.ReportDto utile al PDF) ──

    public sealed class ProjectReportResponseDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public decimal TotalHours { get; set; }
        public int TotalWorkLogs { get; set; }
        public int UniqueEmployees { get; set; }
        public List<WorkLogResponseDto> WorkLogs { get; set; } = new();
    }
}
