using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class StatusService : IStatusService
    {
        private readonly IDbContextService _dbContextService;
        private readonly IMapper _mapper;

        public StatusService(IDbContextService dbContextService, IMapper mapper)
        {
            _dbContextService = dbContextService;
            _mapper = mapper;
        }

        public Task<List<StatusDto.Response>> GetAllAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var list = session.Query<Status>()
                    .OrderBy(s => s.Name)
                    .ToList();
                return _mapper.Map<List<StatusDto.Response>>(list);
            }));
        }

        public Task<StatusDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var s = session.GetObjectByKey<Status>(id);
                return s is null ? null : _mapper.Map<StatusDto.Response>(s);
            }));
        }

        public async Task<StatusDto.Response> CreateAsync(StatusDto.Create dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<StatusDto.Response>(async uow =>
            {
                var entity = new Status(uow)
                {
                    Name = dto.Name
                };
                return _mapper.Map<StatusDto.Response>(entity);
            });
        }

        public async Task<StatusDto.Response?> UpdateAsync(Guid id, StatusDto.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<StatusDto.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Status>(id, ct);
                if (entity is null) return null;
                entity.Name = dto.Name;
                return _mapper.Map<StatusDto.Response>(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Status>(id, ct);
                if (entity is null) return false;

                if (entity.WorkLog != null && entity.WorkLog.Any())
                {
                    throw new InvalidOperationException(
                        $"Impossibile eliminare lo stato '{entity.Name}': esistono {entity.WorkLog.Count} worklog collegati.");
                }

                uow.Delete(entity);
                return true;
            });
        }
    }
}
