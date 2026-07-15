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

        /// <summary>Restituisce null se la rendicontazione non appartiene al dipendente corrente.</summary>
        Task<WorkLogDto.User.Response?> GetByIdAsync(Guid id, Guid currentEmployeeId, CancellationToken ct = default);

        /// <summary>Crea una rendicontazione per il dipendente corrente (Employee impostato dal controller).</summary>
        Task<WorkLogDto.User.Response> CreateAsync(
            WorkLogDto.User.Create dto,
            Guid currentEmployeeId,
            CancellationToken ct = default);

        /// <summary>Restituisce null se la rendicontazione non appartiene al dipendente corrente.</summary>
        Task<WorkLogDto.User.Response?> UpdateAsync(
            Guid id,
            WorkLogDto.User.Update dto,
            Guid currentEmployeeId,
            CancellationToken ct = default);

        /// <summary>Soft delete; false se la rendicontazione non appartiene al dipendente corrente.</summary>
        Task<bool> DeleteAsync(Guid id, Guid currentEmployeeId, CancellationToken ct = default);
    }
}
