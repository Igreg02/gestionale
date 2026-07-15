using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface ICompanyService
    {
        Task<List<CompanyDto.Response>> GetAllAsync(CancellationToken ct = default);
        Task<CompanyDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<CompanyDto.Response> CreateAsync(CompanyDto.Create dto, CancellationToken ct = default);
        Task<CompanyDto.Response?> UpdateAsync(Guid id, CompanyDto.Update dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
