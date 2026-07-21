using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Dataaccess.Helpers;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    /// <summary>
    /// Implementazione di <see cref="IWorkLogUserService"/>: tutte le operazioni
    /// sono filtrate sull'Oid del dipendente autenticato.
    /// </summary>
    public class WorkLogUserService : IWorkLogUserService
    {
        private readonly IDbContextService _dbContextService;
        private readonly IMapper _mapper;

        public WorkLogUserService(IDbContextService dbContextService, IMapper mapper)
        {
            _dbContextService = dbContextService;
            _mapper = mapper;
        }

        public Task<List<WorkLogDto.User.Response>> GetAllAsync(
            Guid currentEmployeeId,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var query = session.Query<WorkLog>()
                    .Active()
                    .Where(w => w.Employee != null
                                && w.Employee.Id == currentEmployeeId);

                if (dateFrom.HasValue)
                {
                    var from = dateFrom.Value;
                    query = query.Where(w => w.Date >= from);
                }

                if (dateTo.HasValue)
                {
                    var to = dateTo.Value;
                    query = query.Where(w => w.Date <= to);
                }

                var list = query
                    .OrderByDescending(w => w.Date)
                    .ThenByDescending(w => w.UpdateAt)
                    .ToList();

                return _mapper.Map<List<WorkLogDto.User.Response>>(list);
            }));
        }

        public Task<WorkLogDto.User.Response?> GetByIdAsync(Guid id, Guid currentEmployeeId, CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var w = session.GetObjectByKey<WorkLog>(id);
                if (w is null || w.IsWorkLogDeleted) return null;
                if (w.Employee == null || w.Employee.Id != currentEmployeeId) return null;
                return _mapper.Map<WorkLogDto.User.Response>(w);
            }));
        }

        public async Task<WorkLogDto.User.Response> CreateAsync(
            WorkLogDto.User.Create dto,
            Guid currentEmployeeId,
            CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<WorkLogDto.User.Response>(async uow =>
            {
                // Lato User ignoriamo dto.IdEmployee e creiamo sempre per il dipendente autenticato.
                var employee = await uow.GetObjectByKeyAsync<Employee>(currentEmployeeId, ct)
                    ?? throw new InvalidOperationException("Dipendente autenticato non trovato");

                var project = await uow.GetObjectByKeyAsync<Project>(dto.IdProject, ct);
                var type = await uow.GetObjectByKeyAsync<Domain.Entities.Type>(dto.IdType, ct);
                var status = await uow.GetObjectByKeyAsync<Status>(dto.IdStatus, ct);

                if (project == null || type == null || status == null)
                    throw new InvalidOperationException("Una delle FK fornite non esiste");

                var now = DateTime.UtcNow;
                var entity = new WorkLog(uow)
                {
                    Description = dto.Description,
                    HoursCounter = dto.HoursCounter,
                    Date = dto.Date,
                    CreateAt = now,
                    UpdateAt = now,
                    IsWorkLogDeleted = false,
                    Project = project,
                    Type = type,
                    Status = status,
                    Employee = employee
                };

                return _mapper.Map<WorkLogDto.User.Response>(entity);
            });
        }

        public async Task<WorkLogDto.User.Response?> UpdateAsync(
            Guid id,
            WorkLogDto.User.Update dto,
            Guid currentEmployeeId,
            CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<WorkLogDto.User.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);
                if (entity == null || entity.IsWorkLogDeleted) return null;
                if (entity.Employee == null || entity.Employee.Id != currentEmployeeId) return null;

                var project = await uow.GetObjectByKeyAsync<Project>(dto.IdProject, ct);
                var type = await uow.GetObjectByKeyAsync<Domain.Entities.Type>(dto.IdType, ct);
                var status = await uow.GetObjectByKeyAsync<Status>(dto.IdStatus, ct);

                if (project == null || type == null || status == null)
                    throw new InvalidOperationException("Una delle FK fornite non esiste");

                entity.Description = dto.Description;
                entity.HoursCounter = dto.HoursCounter;
                entity.Date = dto.Date;
                entity.Project = project;
                entity.Type = type;
                entity.Status = status;
                entity.UpdateAt = DateTime.UtcNow;

                return _mapper.Map<WorkLogDto.User.Response>(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, Guid currentEmployeeId, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);
                if (entity == null || entity.IsWorkLogDeleted) return false;
                if (entity.Employee == null || entity.Employee.Id != currentEmployeeId) return false;

                uow.Delete(entity);
                return true;
            });
        }
    }
}
