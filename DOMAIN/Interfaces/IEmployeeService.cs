using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface IEmployeeService
    {
        Task<List<EmployeeDto.Response>> GetAllAsync(CancellationToken ct = default);
        Task<EmployeeDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<EmployeeDto.Response?> UpdateAsync(Guid id, EmployeeDto.Update dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
