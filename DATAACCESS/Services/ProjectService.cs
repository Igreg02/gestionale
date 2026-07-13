using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IDbContextService _dbContextService;

        public ProjectService(IDbContextService dbContextService)
        {
            _dbContextService = dbContextService;
        }

        private static ProjectDto.Response ToResponse(Project p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            IdCompany = p.Company?.Id ?? Guid.Empty,
            CompanyName = p.Company?.Name ?? string.Empty
        };

        public Task<List<ProjectDto.Response>> GetAllAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
                session.Query<Project>()
                    .OrderBy(p => p.Name)
                    .Select(ToResponse)
                    .ToList()));
        }

        public Task<ProjectDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var p = session.GetObjectByKey<Project>(id);
                return p is null ? null : ToResponse(p);
            }));
        }

        public async Task<ProjectDto.Response> CreateAsync(ProjectDto.Create dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<ProjectDto.Response>(async uow =>
            {
                var company = await uow.GetObjectByKeyAsync<Company>(dto.IdCompany, ct)
                    ?? throw new InvalidOperationException("Azienda non trovata");

                var entity = new Project(uow)
                {
                    Name = dto.Name,
                    Company = company
                };
                await uow.CommitChangesAsync(ct);
                return ToResponse(entity);
            });
        }

        public async Task<ProjectDto.Response?> UpdateAsync(Guid id, ProjectDto.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<ProjectDto.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Project>(id, ct);
                if (entity is null) return null;

                var company = await uow.GetObjectByKeyAsync<Company>(dto.IdCompany, ct)
                    ?? throw new InvalidOperationException("Azienda non trovata");

                entity.Name = dto.Name;
                entity.Company = company;
                await uow.CommitChangesAsync(ct);
                return ToResponse(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Project>(id, ct);
                if (entity is null) return false;
                uow.Delete(entity);
                await uow.CommitChangesAsync(ct);
                return true;
            });
        }
    }
}
