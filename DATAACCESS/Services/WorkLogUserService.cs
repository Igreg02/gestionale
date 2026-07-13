using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    /// <summary>
    /// Implementazione di <see cref="IWorkLogUserService"/>: tutte le operazioni
    /// sono filtrate sull'Oid del dipendente autenticato. Il mapper condiviso
    /// <see cref="WorkLogMapper"/> evita la duplicazione della proiezione.
    /// </summary>
    public class WorkLogUserService : IWorkLogUserService
    {
        private readonly IDbContextService _dbContextService;

        public WorkLogUserService(IDbContextService dbContextService)
        {
            _dbContextService = dbContextService;
        }

        public Task<List<WorkLogAdminDto.Response>> GetAllAsync(
            Guid currentEmployeeOid,
            DateOnly? dateFrom = null,
            DateOnly? dateTo = null,
            CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var query = session.Query<WorkLog>()
                    .Where(w => !w.IsDeleted
                                && w.Employee != null
                                && w.Employee.Oid == currentEmployeeOid);

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

                return query
                    .OrderByDescending(w => w.Date)
                    .ThenByDescending(w => w.UpdateAt)
                    .Select(WorkLogMapper.ToResponse)
                    .ToList();
            }));
        }

        public Task<WorkLogAdminDto.Response?> GetByIdAsync(Guid id, Guid currentEmployeeOid, CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var w = session.GetObjectByKey<WorkLog>(id);
                if (w is null || w.IsDeleted) return null;
                if (w.Employee == null || w.Employee.Oid != currentEmployeeOid) return null;
                return WorkLogMapper.ToResponse(w);
            }));
        }

        public async Task<WorkLogAdminDto.Response> CreateAsync(
            WorkLogAdminDto.Create dto,
            Guid currentEmployeeOid,
            CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<WorkLogAdminDto.Response>(async uow =>
            {
                // Lato User ignoriamo dto.IdEmployee e creiamo sempre per il dipendente autenticato.
                var employee = await uow.GetObjectByKeyAsync<Employee>(currentEmployeeOid, ct)
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
                    IsDeleted = false,
                    Project = project,
                    Type = type,
                    Status = status,
                    Employee = employee
                };

                await uow.CommitChangesAsync(ct);
                return WorkLogMapper.ToResponse(entity);
            });
        }

        public async Task<WorkLogAdminDto.Response?> UpdateAsync(
            Guid id,
            WorkLogAdminDto.Update dto,
            Guid currentEmployeeOid,
            CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<WorkLogAdminDto.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);
                if (entity == null || entity.IsDeleted) return null;
                if (entity.Employee == null || entity.Employee.Oid != currentEmployeeOid) return null;

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
                return WorkLogMapper.ToResponse(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, Guid currentEmployeeOid, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<WorkLog>(id, ct);
                if (entity == null || entity.IsDeleted) return false;
                if (entity.Employee == null || entity.Employee.Oid != currentEmployeeOid) return false;

                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                entity.UpdateAt = DateTime.UtcNow;
                await uow.CommitChangesAsync(ct);
                return true;
            });
        }
    }
}
