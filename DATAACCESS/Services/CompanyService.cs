using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Dataaccess.Helpers;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IDbContextService _dbContextService;
        private readonly IMapper _mapper;

        public CompanyService(IDbContextService dbContextService, IMapper mapper)
        {
            _dbContextService = dbContextService;
            _mapper = mapper;
        }

        public Task<List<CompanyDto.Response>> GetAllAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var list = session.GetAllOrderedBy<Company, string>(c => c.Name);
                return _mapper.Map<List<CompanyDto.Response>>(list);
            }));
        }

        public Task<CompanyDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var c = session.GetObjectByKey<Company>(id);
                return c is null ? null : _mapper.Map<CompanyDto.Response>(c);
            }));
        }

        public async Task<CompanyDto.Response> CreateAsync(CompanyDto.Create dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<CompanyDto.Response>(async uow =>
            {
                var entity = new Company(uow);
                _mapper.Map(dto, entity);
                return _mapper.Map<CompanyDto.Response>(entity);
            }, ct);
        }

        public async Task<CompanyDto.Response?> UpdateAsync(Guid id, CompanyDto.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<CompanyDto.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Company>(id, ct);
                if (entity is null) return null;
                _mapper.Map(dto, entity);
                return _mapper.Map<CompanyDto.Response>(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Company>(id, ct);
                if (entity is null) return false;
                DeleteGuard.ThrowIfHasRelated(
                    entity.Project,
                    $"Impossibile eliminare l'azienda '{entity.Name}': esistono {entity.Project.Count} progetti collegati.");

                uow.Delete(entity);
                return true;
            });
        }
    }
}
