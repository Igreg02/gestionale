using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Dataaccess.Helpers;

namespace GestionaleRendicontazione.Dataaccess.Services
{
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
            return Task.Run(() => _dbContextService.ExecuteReadOnly(session =>
            {
                var obj = session.GetObjectByKey<WorkLog>(id);
                if (obj is null || obj.IsWorkLogDeleted) return null;
                return _mapper.Map<WorkLogDto.Admin.Response>(obj);
            }), ct);
        }

        public Task<List<WorkLogDto.Admin.Response>> GetAllAsync(
            Guid? employeeId = null,
            Guid? projectId = null,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            string? statusName = null,
            CancellationToken ct = default)
        {
            return Task.Run(() => _dbContextService.ExecuteReadOnly(session =>
            {
                var query = session.Query<WorkLog>().Active();

                if (employeeId.HasValue)
                {
                    query = query.Where(w => w.Employee != null && w.Employee.Id == employeeId.Value);
                }

                if (projectId.HasValue)
                {
                    query = query.Where(w => w.Project != null && w.Project.Id == projectId.Value);
                }

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

                if (!string.IsNullOrWhiteSpace(statusName))
                {
                    var trimmed = statusName.Trim();
                    query = query.Where(w => w.Status != null && w.Status.Name == trimmed);
                }

                var list = query
                    .OrderByDescending(w => w.Date)
                    .ThenByDescending(w => w.UpdateAt)
                    .ToList();

                return _mapper.Map<List<WorkLogDto.Admin.Response>>(list);
            }), ct);
        }

        public async Task<WorkLogDto.Admin.Response> CreateAsync(WorkLogDto.Admin.Create dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<WorkLogDto.Admin.Response>(async uow =>
            {
                var project = await uow.GetObjectByKeyAsync<Project>(dto.IdProject, ct);
                var type = await uow.GetObjectByKeyAsync<Domain.Entities.Type>(dto.IdType, ct);
                var status = await uow.GetObjectByKeyAsync<Status>(dto.IdStatus, ct);
                var employee = await uow.GetObjectByKeyAsync<Employee>(dto.IdEmployee, ct);

                if (project == null || type == null || status == null || employee == null)
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

                return _mapper.Map<WorkLogDto.Admin.Response>(entity);
            });
        }

        public async Task<WorkLogDto.Admin.Response?> UpdateAsync(Guid id, WorkLogDto.Admin.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<WorkLogDto.Admin.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);
                if (entity == null || entity.IsWorkLogDeleted) return null;

                var project = await uow.GetObjectByKeyAsync<Project>(dto.IdProject, ct);
                var type = await uow.GetObjectByKeyAsync<Domain.Entities.Type>(dto.IdType, ct);
                var status = await uow.GetObjectByKeyAsync<Status>(dto.IdStatus, ct);
                var employee = await uow.GetObjectByKeyAsync<Employee>(dto.IdEmployee, ct);

                if (project == null || type == null || status == null || employee == null)
                    throw new InvalidOperationException("Una delle FK fornite non esiste");

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
            return await _dbContextService.ReadWrite<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);
                if (entity == null || entity.IsWorkLogDeleted) return false;

                entity.IsWorkLogDeleted = true;
                return true;
            });
        }
    }
}
