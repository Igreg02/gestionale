using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface IWorkLogUserService
    {
        Task<List<WorkLogDto.User.Response>> GetAllAsync(
            Guid currentEmployeeId,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            CancellationToken ct = default);

        Task<WorkLogDto.User.Response?> GetByIdAsync(Guid id, Guid currentEmployeeId, CancellationToken ct = default);

        Task<WorkLogDto.User.Response> CreateAsync(
            WorkLogDto.User.Create dto,
            Guid currentEmployeeId,
            CancellationToken ct = default);

        Task<WorkLogDto.User.Response?> UpdateAsync(
            Guid id,
            WorkLogDto.User.Update dto,
            Guid currentEmployeeId,
            CancellationToken ct = default);

        Task<bool> DeleteAsync(Guid id, Guid currentEmployeeId, CancellationToken ct = default);
    }
}
