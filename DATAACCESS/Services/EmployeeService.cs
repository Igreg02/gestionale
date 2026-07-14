using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IDbContextService _dbContextService;

        public EmployeeService(IDbContextService dbContextService)
        {
            _dbContextService = dbContextService;
        }

        private static EmployeeDto.Response ToResponse(Employee e) => new()
        {
            Oid = e.Oid,
            Username = e.UserName ?? string.Empty,
            FirstName = e.FirstName,
            LastName = e.LastName
        };

        public Task<List<EmployeeDto.Response>> GetAllAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
                session.Query<Employee>()
                    .OrderBy(e => e.UserName)
                    .Select(ToResponse)
                    .ToList()));
        }

        public Task<EmployeeDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var e = session.GetObjectByKey<Employee>(id);
                return e is null ? null : ToResponse(e);
            }));
        }

        public async Task<EmployeeDto.Response?> UpdateAsync(Guid id, EmployeeDto.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<EmployeeDto.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Employee>(id, ct);
                if (entity is null) return null;

                entity.UserName = dto.Username;
                entity.FirstName = dto.FirstName;
                entity.LastName = dto.LastName;
                await uow.CommitChangesAsync(ct);
                return ToResponse(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Employee>(id, ct);
                if (entity is null) return false;
                uow.Delete(entity);
                await uow.CommitChangesAsync(ct);
                return true;
            });
        }
    }
}
