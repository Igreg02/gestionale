using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using DevExpress.Xpo;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class WorkLogAdminService : IWorkLogService
    {
        private readonly IDbContextService _dbContextService;

        public WorkLogAdminService(IDbContextService dbContextService)
        {
            _dbContextService = dbContextService;
        }

        public async Task<WorkLogAdminDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContextService.ExecuteReadOnly(async uow =>
            {
                var obj = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);

                if (obj == null)
                    return null; //TODO: aggiungere exceptions

                return new WorkLogAdminDto.Response
                {
                    Id = obj.Id,
                    Description = obj.Description,
                    HoursCounter = obj.HoursCounter,
                    Date = obj.Date,
                    CreateAt = obj.CreateAt,
                    UpdateAt = obj.UpdateAt,
                    IdProject = obj.Project.Id,
                    ProjectName = obj.Project.Name,
                    IdEmploy = obj.Employee?.Oid ?? Guid.Empty,
                    EmployeeName = obj.Employee?.UserName ?? string.Empty,
                    IdType = obj.Type.Id,
                    TypeName = obj.Type.Name,
                    IdStatus = obj.Status.Id,
                    StatusName = obj.Status.Name
                };
            });
        }

        public async Task<List<WorkLogAdminDto.Response>> GetAllAsync(CancellationToken ct = default)
        {
            return await _dbContextService.ExecuteReadOnly(async uow =>
            {
                var list = await uow.Query<WorkLog>().ToListAsync(ct);

                if (list == null)
                    return new List<WorkLogAdminDto.Response>(); //TODO: aggiungere exceptions

                return list.Select(obj => new WorkLogAdminDto.Response
                {
                    Id = obj.Id,
                    Description = obj.Description,
                    HoursCounter = obj.HoursCounter,
                    Date = obj.Date,
                    CreateAt = obj.CreateAt,
                    UpdateAt = obj.UpdateAt,
                    IdProject = obj.Project.Id,
                    ProjectName = obj.Project.Name,
                    IdEmploy = obj.Employee?.Oid ?? Guid.Empty,
                    EmployeeName = obj.Employee?.UserName ?? string.Empty,
                    IdType = obj.Type.Id,
                    TypeName = obj.Type.Name,
                    IdStatus = obj.Status.Id,
                    StatusName = obj.Status.Name
                }).ToList();
            });
        }

        public async Task<WorkLogAdminDto.Response> CreateAsync(WorkLogAdminDto.Create dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<WorkLogAdminDto.Response>(async uow =>
            {
                if (await uow.Query<WorkLog>()
                    .FirstOrDefaultAsync(w =>
                        w.Description == dto.Description &&
                        w.Date == dto.Date &&
                        w.Employee.Oid == dto.IdEmployee,
                        ct) != null)
                    throw new InvalidOperationException("WorkLog already exists");

                var project = await uow.GetObjectByKeyAsync<Project>(dto.IdProject, ct);
                var type = await uow.GetObjectByKeyAsync<Domain.Entities.Type>(dto.IdType, ct);
                var status = await uow.GetObjectByKeyAsync<Status>(dto.IdStatus, ct);
                var employee = await uow.GetObjectByKeyAsync<Employee>(dto.IdEmployee, ct);

                if (project == null || type == null || status == null || employee == null)
                    throw new InvalidOperationException("Una delle FK fornite non esiste");

                var entity = new WorkLog(uow)
                {
                    Description = dto.Description,
                    HoursCounter = dto.HoursCounter,
                    Date = dto.Date,
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow,
                    Project = project,
                    Type = type,
                    Status = status,
                    Employee = employee
                };

                await uow.CommitChangesAsync(ct);

                return new WorkLogAdminDto.Response
                {
                    Id = entity.Id,
                    Description = entity.Description,
                    HoursCounter = entity.HoursCounter,
                    Date = entity.Date,
                    CreateAt = entity.CreateAt,
                    UpdateAt = entity.UpdateAt,
                    IdProject = entity.Project.Id,
                    ProjectName = entity.Project.Name,
                    IdEmploy = entity.Employee.Oid,
                    EmployeeName = entity.Employee.UserName,
                    IdType = entity.Type.Id,
                    TypeName = entity.Type.Name,
                    IdStatus = entity.Status.Id,
                    StatusName = entity.Status.Name
                };
            });
        }

        public async Task<WorkLogAdminDto.Response?> UpdateAsync(Guid id, WorkLogAdminDto.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<WorkLogAdminDto.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);
                if (entity == null)
                    return null;

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

                await uow.CommitChangesAsync(ct);

                return new WorkLogAdminDto.Response
                {
                    Id = entity.Id,
                    Description = entity.Description,
                    HoursCounter = entity.HoursCounter,
                    Date = entity.Date,
                    CreateAt = entity.CreateAt,
                    UpdateAt = entity.UpdateAt,
                    IdProject = entity.Project.Id,
                    ProjectName = entity.Project.Name,
                    IdEmploy = entity.Employee?.Oid ?? Guid.Empty,
                    EmployeeName = entity.Employee?.UserName ?? string.Empty,
                    IdType = entity.Type.Id,
                    TypeName = entity.Type.Name,
                    IdStatus = entity.Status.Id,
                    StatusName = entity.Status.Name
                };
            });
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);
                if (entity == null)
                    return false;

                uow.Delete(entity);
                await uow.CommitChangesAsync(ct);
                return true;
            });
        }
    }
}
