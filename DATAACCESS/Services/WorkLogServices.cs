using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Dataaccess.Helpers;

namespace GestionaleRendicontazione.Dataaccess.Services
{
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
                var obj = session.GetObjectByKey<WorkLog>(id);
                if (obj is null || obj.IsWorkLogDeleted) return null;
                return _mapper.Map<WorkLogDto.Admin.Response>(obj);
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
            return await _dbContextService.ReadWriteAsync<WorkLogDto.Admin.Response>(async uow =>
            {
                var entity = new WorkLog(uow);
                _mapper.Map(dto, entity, opt => opt.Items[WorkLogMappingContextKeys.Uow] = uow);

                return _mapper.Map<WorkLogDto.Admin.Response>(entity);
            });
        }

        public async Task<WorkLogDto.Admin.Response?> UpdateAsync(Guid id, WorkLogDto.Admin.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<WorkLogDto.Admin.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);
                if (entity == null || entity.IsWorkLogDeleted) return null;

                _mapper.Map(dto, entity, opt => opt.Items[WorkLogMappingContextKeys.Uow] = uow);

                return _mapper.Map<WorkLogDto.Admin.Response>(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);
                if (entity == null || entity.IsWorkLogDeleted) return false;

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
            return await _dbContextService.ReadWriteAsync<WorkLogDto.User.Response>(async uow =>
            {
                // Il dipendente è SEMPRE quello autenticato (il DTO non lo porta).
                var employee = await uow.GetRequiredAsync<Employee>(
                    currentEmployeeId, "Dipendente autenticato non trovato", ct);

                var entity = new WorkLog(uow);
                _mapper.Map(dto, entity, opt =>
                {
                    opt.Items[WorkLogMappingContextKeys.Uow] = uow;
                    opt.Items[WorkLogMappingContextKeys.CurrentEmployee] = employee;
                });

                return _mapper.Map<WorkLogDto.User.Response>(entity);
            });
        }

        public async Task<WorkLogDto.User.Response?> UpdateAsync(
            Guid id,
            WorkLogDto.User.Update dto,
            Guid currentEmployeeId,
            CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<WorkLogDto.User.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);
                if (entity == null || entity.IsWorkLogDeleted) return null;
                if (entity.Employee == null || entity.Employee.Id != currentEmployeeId) return null;

                _mapper.Map(dto, entity, opt => opt.Items[WorkLogMappingContextKeys.Uow] = uow);

                return _mapper.Map<WorkLogDto.User.Response>(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, Guid currentEmployeeId, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<bool>(async uow =>
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
