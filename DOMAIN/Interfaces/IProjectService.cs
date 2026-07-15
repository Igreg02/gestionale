using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface IProjectService
    {
        Task<List<ProjectDto.Response>> GetAllAsync(CancellationToken ct = default);
        Task<ProjectDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<ProjectDto.Response> CreateAsync(ProjectDto.Create dto, CancellationToken ct = default);
        Task<ProjectDto.Response?> UpdateAsync(Guid id, ProjectDto.Update dto, CancellationToken ct = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
