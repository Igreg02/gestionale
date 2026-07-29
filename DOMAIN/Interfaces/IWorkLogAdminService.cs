using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface IWorkLogAdminService
    {
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

        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
