using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class TypeService : ITypeService
    {
        private readonly IDbContextService _dbContextService;

        public TypeService(IDbContextService dbContextService)
        {
            _dbContextService = dbContextService;
        }

        private static TypeDto.Response ToResponse(Domain.Entities.Type t) => new()
        {
            Id = t.Id,
            Name = t.Name
        };

        public Task<List<TypeDto.Response>> GetAllAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
                session.Query<Domain.Entities.Type>()
                    .OrderBy(t => t.Name)
                    .Select(ToResponse)
                    .ToList()));
        }

        public Task<TypeDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var t = session.GetObjectByKey<Domain.Entities.Type>(id);
                return t is null ? null : ToResponse(t);
            }));
        }

        public async Task<TypeDto.Response> CreateAsync(TypeDto.Create dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<TypeDto.Response>(async uow =>
            {
                var entity = new Domain.Entities.Type(uow)
                {
                    Name = dto.Name
                };
                await uow.CommitChangesAsync(ct);
                return ToResponse(entity);
            });
        }

        public async Task<TypeDto.Response?> UpdateAsync(Guid id, TypeDto.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<TypeDto.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Domain.Entities.Type>(id, ct);
                if (entity is null) return null;
                entity.Name = dto.Name;
                await uow.CommitChangesAsync(ct);
                return ToResponse(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Domain.Entities.Type>(id, ct);
                if (entity is null) return false;
                uow.Delete(entity);
                await uow.CommitChangesAsync(ct);
                return true;
            });
        }
    }
}
