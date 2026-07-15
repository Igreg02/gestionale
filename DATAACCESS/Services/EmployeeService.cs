using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IDbContextService _dbContextService;
        private readonly IMapper _mapper;

        public EmployeeService(IDbContextService dbContextService, IMapper mapper)
        {
            _dbContextService = dbContextService;
            _mapper = mapper;
        }

        public Task<List<EmployeeDto.Response>> GetAllAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var list = session.Query<Employee>()
                    .OrderBy(e => e.UserName)
                    .ToList();
                return _mapper.Map<List<EmployeeDto.Response>>(list);
            }));
        }

        public Task<EmployeeDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var e = session.GetObjectByKey<Employee>(id);
                return e is null ? null : _mapper.Map<EmployeeDto.Response>(e);
            }));
        }

        public async Task<EmployeeDto.Response?> UpdateAsync(Guid id, EmployeeDto.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<EmployeeDto.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Employee>(id, ct);
                if (entity is null) return null;
                if (!string.Equals(entity.UserName, dto.Username, StringComparison.Ordinal))
                {
                    var usernameTaken = uow.Query<Employee>()
                        .Any(e => e.UserName == dto.Username && e.Id != id);
                    if (usernameTaken)
                    {
                        throw new InvalidOperationException(
                            $"Lo username '{dto.Username}' è già utilizzato da un altro dipendente.");
                    }
                }

                _mapper.Map(dto, entity);
                return _mapper.Map<EmployeeDto.Response>(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Employee>(id, ct);
                if (entity is null) return false;

                if (entity.WorkLogs != null && entity.WorkLogs.Any())
                {
                    throw new InvalidOperationException(
                        $"Impossibile eliminare il dipendente '{entity.UserName}': esistono {entity.WorkLogs.Count} worklog associati.");
                }

                uow.Delete(entity);
                return true;
            });
        }
    }
}
