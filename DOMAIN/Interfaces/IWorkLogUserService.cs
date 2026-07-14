using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface IWorkLogUserService
    {
        Task<List<WorkLogAdminDto.Response>> GetAllAsync(
            Guid currentEmployeeId,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            CancellationToken ct = default);

        /// <summary>Restituisce null se la rendicontazione non appartiene al dipendente corrente.</summary>
        Task<WorkLogAdminDto.Response?> GetByIdAsync(Guid id, Guid currentEmployeeId, CancellationToken ct = default);

        /// <summary>Crea una rendicontazione per il dipendente corrente (Employee impostato dal controller).</summary>
        Task<WorkLogAdminDto.Response> CreateAsync(
            WorkLogAdminDto.Create dto,
            Guid currentEmployeeId,
            CancellationToken ct = default);

        /// <summary>Restituisce null se la rendicontazione non appartiene al dipendente corrente.</summary>
        Task<WorkLogAdminDto.Response?> UpdateAsync(
            Guid id,
            WorkLogAdminDto.Update dto,
            Guid currentEmployeeId,
            CancellationToken ct = default);

        /// <summary>Soft delete; false se la rendicontazione non appartiene al dipendente corrente.</summary>
        Task<bool> DeleteAsync(Guid id, Guid currentEmployeeId, CancellationToken ct = default);
    }
}
