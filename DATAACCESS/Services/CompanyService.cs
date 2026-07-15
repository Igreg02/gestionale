using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

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
            return Task.Run(() => _dbContextService.ExecuteReadOnly(session =>
            {
                var list = session.Query<Company>()
                    .OrderBy(c => c.Name)
                    .ToList();
                return _mapper.Map<List<CompanyDto.Response>>(list);
            }), ct);
        }

        public Task<CompanyDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.Run(() => _dbContextService.ExecuteReadOnly(session =>
            {
                var c = session.GetObjectByKey<Company>(id);
                return c is null ? null : _mapper.Map<CompanyDto.Response>(c);
            }), ct);
        }

        public async Task<CompanyDto.Response> CreateAsync(CompanyDto.Create dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<CompanyDto.Response>(async uow =>
            {
                var entity = new Company(uow);
                _mapper.Map(dto, entity);
                return _mapper.Map<CompanyDto.Response>(entity);
            });
        }

        public async Task<CompanyDto.Response?> UpdateAsync(Guid id, CompanyDto.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<CompanyDto.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Company>(id, ct);
                if (entity is null) return null;
                _mapper.Map(dto, entity);
                return _mapper.Map<CompanyDto.Response>(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Company>(id, ct);
                if (entity is null) return false;
                if (entity.Project != null && entity.Project.Any())
                {
                    throw new InvalidOperationException(
                        $"Impossibile eliminare l'azienda '{entity.Name}': esistono {entity.Project.Count} progetti collegati.");
                }

                uow.Delete(entity);
                return true;
            });
        }
    }
}
