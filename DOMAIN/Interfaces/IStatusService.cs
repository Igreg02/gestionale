using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface IStatusService
    {
        Task<List<StatusDto.Response>> GetAllAsync(CancellationToken ct = default);
        Task<StatusDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<StatusDto.Response> CreateAsync(StatusDto.Create dto, CancellationToken ct = default);
        Task<StatusDto.Response?> UpdateAsync(Guid id, StatusDto.Update dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
