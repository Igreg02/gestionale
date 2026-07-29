using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface IWorkLogAdminService
    {
        /// <summary>
        /// Filtri supportati: EmployeeId, ProjectId, DateFrom, DateTo, StatusId, StatusName.
        /// I parametri nullable indicano "filtro non applicato".
        /// </summary>
        Task<List<WorkLogDto.Admin.Response>> GetAllAsync(
            Guid? employeeId = null,
            Guid? projectId = null,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            string? statusName = null,
            CancellationToken ct = default);

        Task<WorkLogDto.Admin.Response?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<WorkLogDto.Admin.Response> CreateAsync(WorkLogDto.Admin.Create dto, CancellationToken ct = default);

        Task<WorkLogDto.Admin.Response?> UpdateAsync(Guid id, WorkLogDto.Admin.Update dto, CancellationToken ct = default);

        /// <summary>Soft delete: imposta IsWorkLogDeleted (campo custom di WorkLog).</summary>
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
