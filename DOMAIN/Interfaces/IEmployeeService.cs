using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto.Response>> GetAllAsync(CancellationToken ct = default);
        Task<EmployeeDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<EmployeeDto.Response?> UpdateAsync(Guid id, EmployeeDto.Update dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Forza <see cref="Domain.Entities.Employee.MustChangePassword"/> a true sull'employee indicato
        /// (azione Admin dalla gestione dipendenti). Ritorna false se l'employee non esiste.
        /// </summary>
        Task<bool> ForcePasswordResetAsync(Guid id, CancellationToken ct = default);
    }
}
