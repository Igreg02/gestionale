using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface IWorkLogAdminService
    {
        /// <summary>
        /// Filtri supportati: EmployeeId, ProjectId, DateFrom, DateTo, StatusId, StatusName.
        /// I parametri nullable indicano "filtro non applicato".
        /// </summary>
        Task<List<WorkLogAdminDto.Response>> GetAllAsync(
            Guid? employeeId = null,
            Guid? projectId = null,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            string? statusName = null,
            CancellationToken ct = default);

        Task<WorkLogAdminDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<WorkLogAdminDto.Response> CreateAsync(WorkLogAdminDto.Create dto, CancellationToken ct = default);

        Task<WorkLogAdminDto.Response?> UpdateAsync(Guid id, WorkLogAdminDto.Update dto, CancellationToken ct = default);

        /// <summary>Soft delete: imposta IsDeleted + DeletedAt.</summary>
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
