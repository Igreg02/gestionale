using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface ITypeService
    {
        Task<List<TypeDto.Response>> GetAllAsync(CancellationToken ct = default);
        Task<TypeDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<TypeDto.Response> CreateAsync(TypeDto.Create dto, CancellationToken ct = default);
        Task<TypeDto.Response?> UpdateAsync(Guid id, TypeDto.Update dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
