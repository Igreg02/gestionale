using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    /// <summary>
    /// Servizio di reportistica: aggregazioni per progetto/periodo e per dipendente/periodo.
    /// Restituisce null quando l'FK (progetto o dipendente) non esiste.
    /// </summary>
    public interface IReportService
    {
        Task<ProjectReportDto.Response?> GetProjectReportAsync(
            Guid projectId,
            DateOnly from,
            DateOnly to,
            CancellationToken ct = default);

        Task<EmployeeReportDto.Response?> GetEmployeeReportAsync(
            Guid employeeId,
            DateOnly from,
            DateOnly to,
            CancellationToken ct = default);
    }
}
