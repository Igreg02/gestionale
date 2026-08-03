using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Dataaccess.Helpers;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    /// <summary>
    /// Lookup condiviso tra <see cref="WorkLogAdminService"/> e <see cref="WorkLogUserService"/>:
    /// entrambi devono scartare i worklog soft-deleted, e la versione utente in più deve
    /// verificare che il worklog appartenga al dipendente autenticato. Centralizzato qui per
    /// evitare che le due classi ripetano lo stesso controllo in ogni singolo metodo CRUD.
    /// </summary>
    internal static class WorkLogAccess
    {
        public static WorkLog? FindActive(Session session, Guid id, Guid? restrictToEmployeeId = null)
        {
            var w = session.GetObjectByKey<WorkLog>(id);
            return IsAccessible(w, restrictToEmployeeId) ? w : null;
        }

        public static async Task<WorkLog?> FindActiveAsync(UnitOfWork uow, Guid id, Guid? restrictToEmployeeId, CancellationToken ct)
        {
            var w = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);
            return IsAccessible(w, restrictToEmployeeId) ? w : null;
        }

        private static bool IsAccessible(WorkLog? w, Guid? restrictToEmployeeId)
        {
            if (w is null || w.IsWorkLogDeleted) return false;
            if (restrictToEmployeeId is { } ownerId && (w.Employee is null || w.Employee.Id != ownerId)) return false;
            return true;
        }
    }

    /// <summary>
    /// Implementazione di <see cref="IWorkLogAdminService"/>: nessun filtro sul dipendente,
    /// accesso completo a tutti i worklog.
    /// </summary>
    public class WorkLogAdminService : IWorkLogAdminService
    {
        private readonly IDbContextService _dbContextService;
        private readonly IMapper _mapper;

        public WorkLogAdminService(IDbContextService dbContextService, IMapper mapper)
        {
            _dbContextService = dbContextService;
            _mapper = mapper;
        }

        public Task<WorkLogDto.Admin.Response?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var obj = WorkLogAccess.FindActive(session, id);
                return obj is null ? null : _mapper.Map<WorkLogDto.Admin.Response>(obj);
            }));
        }

        public Task<List<WorkLogDto.Admin.Response>> GetAllAsync(
            Guid? employeeId = null,
            Guid? projectId = null,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            string? statusName = null,
            CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var query = session.Query<WorkLog>().Active()
                    .ApplyFilters(employeeId, projectId, dateFrom, dateTo, statusName);

                var list = query
                    .OrderByDescending(w => w.Date)
                    .ThenByDescending(w => w.UpdateAt)
                    .ToList();

                return _mapper.Map<List<WorkLogDto.Admin.Response>>(list);
            }));
        }

        public async Task<WorkLogDto.Admin.Response> CreateAsync(WorkLogDto.Admin.Create dto, CancellationToken ct = default)
        {
            // La Guid key [Key(AutoGenerate = true)] di WorkLog viene generata da XPO solo al
            // commit fisico: si mappa la Response solo DOPO che ReadWriteAsync ha committato,
            // altrimenti Id risulterebbe sempre Guid.Empty (Cf. XpoCrudServiceBase.CreateAsync).
            var entity = await _dbContextService.ReadWriteAsync<WorkLog>(async uow =>
            {
                var newEntity = new WorkLog(uow);
                _mapper.Map(dto, newEntity, opt => opt.Items[WorkLogMappingContextKeys.Uow] = uow);
                return newEntity;
            });

            return _mapper.Map<WorkLogDto.Admin.Response>(entity);
        }

        public async Task<WorkLogDto.Admin.Response?> UpdateAsync(Guid id, WorkLogDto.Admin.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<WorkLogDto.Admin.Response?>(async uow =>
            {
                var entity = await WorkLogAccess.FindActiveAsync(uow, id, restrictToEmployeeId: null, ct);
                if (entity is null) return null;

                _mapper.Map(dto, entity, opt => opt.Items[WorkLogMappingContextKeys.Uow] = uow);

                return _mapper.Map<WorkLogDto.Admin.Response>(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<bool>(async uow =>
            {
                var entity = await WorkLogAccess.FindActiveAsync(uow, id, restrictToEmployeeId: null, ct);
                if (entity is null) return false;

                uow.Delete(entity);
                return true;
            });
        }
    }

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
                                && w.Employee.Id == currentEmployeeId)
                    .ApplyFilters(employeeId: null, projectId: null, dateFrom, dateTo);

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
                var w = WorkLogAccess.FindActive(session, id, currentEmployeeId);
                return w is null ? null : _mapper.Map<WorkLogDto.User.Response>(w);
            }));
        }

        public async Task<WorkLogDto.User.Response> CreateAsync(
            WorkLogDto.User.Create dto,
            Guid currentEmployeeId,
            CancellationToken ct = default)
        {
            // Id valorizzato solo dopo il commit fatto da ReadWriteAsync: si mappa la Response
            // dopo, non dentro il delegate (Cf. XpoCrudServiceBase.CreateAsync).
            var entity = await _dbContextService.ReadWriteAsync<WorkLog>(async uow =>
            {
                // Il dipendente è SEMPRE quello autenticato (il DTO non lo porta).
                var employee = await uow.GetRequiredAsync<Employee>(
                    currentEmployeeId, "Dipendente autenticato non trovato", ct);

                var newEntity = new WorkLog(uow);
                _mapper.Map(dto, newEntity, opt =>
                {
                    opt.Items[WorkLogMappingContextKeys.Uow] = uow;
                    opt.Items[WorkLogMappingContextKeys.CurrentEmployee] = employee;
                });

                return newEntity;
            });

            return _mapper.Map<WorkLogDto.User.Response>(entity);
        }

        public async Task<WorkLogDto.User.Response?> UpdateAsync(
            Guid id,
            WorkLogDto.User.Update dto,
            Guid currentEmployeeId,
            CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<WorkLogDto.User.Response?>(async uow =>
            {
                var entity = await WorkLogAccess.FindActiveAsync(uow, id, currentEmployeeId, ct);
                if (entity is null) return null;

                _mapper.Map(dto, entity, opt => opt.Items[WorkLogMappingContextKeys.Uow] = uow);

                return _mapper.Map<WorkLogDto.User.Response>(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, Guid currentEmployeeId, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<bool>(async uow =>
            {
                var entity = await WorkLogAccess.FindActiveAsync(uow, id, currentEmployeeId, ct);
                if (entity is null) return false;

                uow.Delete(entity);
                return true;
            });
        }
    }
}
