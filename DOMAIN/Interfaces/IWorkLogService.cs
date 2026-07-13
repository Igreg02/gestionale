using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface IWorkLogService
    {
        Task<WorkLogAdminDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<List<WorkLogAdminDto.Response>> GetAllAsync(CancellationToken ct = default);

        Task<WorkLogAdminDto.Response> CreateAsync(WorkLogAdminDto.Create dto, CancellationToken ct = default);

        Task<WorkLogAdminDto.Response?> UpdateAsync(Guid id, WorkLogAdminDto.Update dto, CancellationToken ct = default);

        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
