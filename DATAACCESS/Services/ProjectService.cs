using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Dataaccess.Helpers;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IDbContextService _dbContextService;
        private readonly IMapper _mapper;

        public ProjectService(IDbContextService dbContextService, IMapper mapper)
        {
            _dbContextService = dbContextService;
            _mapper = mapper;
        }

        public Task<List<ProjectDto.Response>> GetAllAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var list = session.GetAllOrderedBy<Project, string>(p => p.Name);
                return _mapper.Map<List<ProjectDto.Response>>(list);
            }));
        }

        public Task<ProjectDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var p = session.GetObjectByKey<Project>(id);
                return p is null ? null : _mapper.Map<ProjectDto.Response>(p);
            }));
        }

        public async Task<ProjectDto.Response> CreateAsync(ProjectDto.Create dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<ProjectDto.Response>(async uow =>
            {
                var company = await uow.GetRequiredObjectByKeyAsync<Company>(dto.IdCompany, "Azienda", ct);

                var entity = new Project(uow)
                {
                    Name = dto.Name,
                    Company = company
                };
                return _mapper.Map<ProjectDto.Response>(entity);
            });
        }

        public async Task<ProjectDto.Response?> UpdateAsync(Guid id, ProjectDto.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<ProjectDto.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Project>(id, ct);
                if (entity is null) return null;

                var company = await uow.GetRequiredObjectByKeyAsync<Company>(dto.IdCompany, "Azienda", ct);

                entity.Name = dto.Name;
                entity.Company = company;
                return _mapper.Map<ProjectDto.Response>(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Project>(id, ct);
                if (entity is null) return false;

                DeleteGuard.ThrowIfHasRelated(entity.WorkLog, "il progetto", entity.Name);

                uow.Delete(entity);
                return true;
            });
        }
    }
}
