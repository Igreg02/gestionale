using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class TypeService : ITypeService
    {
        private readonly IDbContextService _dbContextService;
        private readonly IMapper _mapper;

        public TypeService(IDbContextService dbContextService, IMapper mapper)
        {
            _dbContextService = dbContextService;
            _mapper = mapper;
        }

        public Task<List<TypeDto.Response>> GetAllAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var list = session.Query<Domain.Entities.Type>()
                    .OrderBy(t => t.Name)
                    .ToList();
                return _mapper.Map<List<TypeDto.Response>>(list);
            }));
        }

        public Task<TypeDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var t = session.GetObjectByKey<Domain.Entities.Type>(id);
                return t is null ? null : _mapper.Map<TypeDto.Response>(t);
            }));
        }

        public async Task<TypeDto.Response> CreateAsync(TypeDto.Create dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<TypeDto.Response>(async uow =>
            {
                var entity = new Domain.Entities.Type(uow)
                {
                    Name = dto.Name
                };
                return _mapper.Map<TypeDto.Response>(entity);
            });
        }

        public async Task<TypeDto.Response?> UpdateAsync(Guid id, TypeDto.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<TypeDto.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Domain.Entities.Type>(id, ct);
                if (entity is null) return null;
                entity.Name = dto.Name;
                return _mapper.Map<TypeDto.Response>(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Domain.Entities.Type>(id, ct);
                if (entity is null) return false;

                if (entity.WorkLog != null && entity.WorkLog.Any())
                {
                    throw new InvalidOperationException(
                        $"Impossibile eliminare il tipo '{entity.Name}': esistono {entity.WorkLog.Count} worklog collegati.");
                }

                uow.Delete(entity);
                return true;
            });
        }
    }
}
