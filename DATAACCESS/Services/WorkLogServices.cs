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
                var project = await uow.GetRequiredAsync<Project>(dto.IdProject, "Una delle FK fornite non esiste", ct);
                var type = await uow.GetRequiredAsync<Domain.Entities.Type>(dto.IdType, "Una delle FK fornite non esiste", ct);
                var status = await uow.GetRequiredAsync<Status>(dto.IdStatus, "Una delle FK fornite non esiste", ct);
                var employee = await uow.GetRequiredAsync<Employee>(dto.IdEmployee, "Una delle FK fornite non esiste", ct);

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

                return _mapper.Map<WorkLogDto.Admin.Response>(entity);
            });
        }

        public async Task<WorkLogDto.Admin.Response?> UpdateAsync(Guid id, WorkLogDto.Admin.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWriteAsync<WorkLogDto.Admin.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);
                if (entity == null || entity.IsWorkLogDeleted) return null;

                var project = await uow.GetRequiredAsync<Project>(dto.IdProject, "Una delle FK fornite non esiste", ct);
                var type = await uow.GetRequiredAsync<Domain.Entities.Type>(dto.IdType, "Una delle FK fornite non esiste", ct);
                var status = await uow.GetRequiredAsync<Status>(dto.IdStatus, "Una delle FK fornite non esiste", ct);
                var employee = await uow.GetRequiredAsync<Employee>(dto.IdEmployee, "Una delle FK fornite non esiste", ct);

                entity.Description = dto.Description;
                entity.HoursCounter = dto.HoursCounter;
                entity.Date = dto.Date;
                entity.Project = project;
                entity.Type = type;
                entity.Status = status;
                entity.Employee = employee;
                entity.UpdateAt = DateTime.UtcNow;

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
                // Lato User ignoriamo dto.IdEmployee e creiamo sempre per il dipendente autenticato.
                var employee = await uow.GetRequiredAsync<Employee>(currentEmployeeId, "Dipendente autenticato non trovato", ct);

                var project = await uow.GetRequiredAsync<Project>(dto.IdProject, "Una delle FK fornite non esiste", ct);
                var type = await uow.GetRequiredAsync<Domain.Entities.Type>(dto.IdType, "Una delle FK fornite non esiste", ct);
                var status = await uow.GetRequiredAsync<Status>(dto.IdStatus, "Una delle FK fornite non esiste", ct);

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
            return await _dbContextService.ReadWriteAsync<WorkLogDto.User.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);
                if (entity == null || entity.IsWorkLogDeleted) return null;
                if (entity.Employee == null || entity.Employee.Id != currentEmployeeId) return null;

                var project = await uow.GetRequiredAsync<Project>(dto.IdProject, "Una delle FK fornite non esiste", ct);
                var type = await uow.GetRequiredAsync<Domain.Entities.Type>(dto.IdType, "Una delle FK fornite non esiste", ct);
                var status = await uow.GetRequiredAsync<Status>(dto.IdStatus, "Una delle FK fornite non esiste", ct);

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
