using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface IReportService
    {
        Task<ReportDto.Project.Response?> GetProjectReportAsync(
            Guid projectId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken ct = default);

        Task<ReportDto.Employee.Response?> GetEmployeeReportAsync(
            Guid employeeId,
            DateOnly? from,
            DateOnly? to,
            CancellationToken ct = default);
    }
}
