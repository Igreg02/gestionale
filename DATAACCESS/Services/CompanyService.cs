using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly IDbContextService _dbContextService;

        public CompanyService(IDbContextService dbContextService)
        {
            _dbContextService = dbContextService;
        }

        private static CompanyDto.Response ToResponse(Company c) => new()
        {
            id = c.Id,
            Name = c.Name,
            email = c.Email
        };

        public Task<List<CompanyDto.Response>> GetAllAsync(CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
                session.Query<Company>()
                    .OrderBy(c => c.Name)
                    .Select(ToResponse)
                    .ToList()));
        }

        public Task<CompanyDto.Response?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var c = session.GetObjectByKey<Company>(id);
                return c is null ? null : ToResponse(c);
            }));
        }

        public async Task<CompanyDto.Response> CreateAsync(CompanyDto.Create dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<CompanyDto.Response>(async uow =>
            {
                var entity = new Company(uow)
                {
                    Name = dto.Name,
                    Email = dto.email
                };
                await uow.CommitChangesAsync(ct);
                return ToResponse(entity);
            });
        }

        public async Task<CompanyDto.Response?> UpdateAsync(Guid id, CompanyDto.Update dto, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<CompanyDto.Response?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Company>(id, ct);
                if (entity is null) return null;
                entity.Name = dto.Name;
                entity.Email = dto.email;
                await uow.CommitChangesAsync(ct);
                return ToResponse(entity);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await _dbContextService.ReadWrite<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<Company>(id, ct);
                if (entity is null) return false;
                uow.Delete(entity);
                await uow.CommitChangesAsync(ct);
                return true;
            });
        }
    }
}
