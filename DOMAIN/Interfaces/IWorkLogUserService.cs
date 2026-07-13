using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    /// <summary>
    /// WorkLog service limitato al dipendente corrente. Tutte le operazioni di lettura/scrittura
    /// sono filtrate sull'Oid del dipendente autenticato (derivato dal claim "sub" del JWT).
    /// </summary>
    public interface IWorkLogUserService
    {
        Task<List<WorkLogAdminDto.Response>> GetAllAsync(
            Guid currentEmployeeOid,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            CancellationToken ct = default);

        /// <summary>Restituisce null se la rendicontazione non appartiene al dipendente corrente.</summary>
        Task<WorkLogAdminDto.Response?> GetByIdAsync(Guid id, Guid currentEmployeeOid, CancellationToken ct = default);

        /// <summary>Crea una rendicontazione per il dipendente corrente (Employee impostato dal controller).</summary>
        Task<WorkLogAdminDto.Response> CreateAsync(
            WorkLogAdminDto.Create dto,
            Guid currentEmployeeOid,
            CancellationToken ct = default);

        /// <summary>Restituisce null se la rendicontazione non appartiene al dipendente corrente.</summary>
        Task<WorkLogAdminDto.Response?> UpdateAsync(
            Guid id,
            WorkLogAdminDto.Update dto,
            Guid currentEmployeeOid,
            CancellationToken ct = default);

        /// <summary>Soft delete; false se la rendicontazione non appartiene al dipendente corrente.</summary>
        Task<bool> DeleteAsync(Guid id, Guid currentEmployeeOid, CancellationToken ct = default);
    }
}
