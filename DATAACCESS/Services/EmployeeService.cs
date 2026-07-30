using System.Linq.Expressions;
using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Dataaccess.Services.Abstractions;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{

    public class EmployeeService
        : XpoCrudServiceBaseNoCreate<Employee, EmployeeDto.Response, EmployeeDto.Update>,
          IEmployeeService
    {
        public EmployeeService(IDbContextService db, IMapper mapper) : base(db, mapper) { }

        protected override Employee CreateEntity(UnitOfWork uow) => new Employee(uow);

        protected override Expression<Func<Employee, string>> OrderByExpr => e => e.UserName;

        protected override string EntityKindSingular => "il dipendente";

        protected override string EntityLogName(Employee entity) => entity.UserName;

        protected override int? GetRelatedChildrenCount(Employee entity) => entity.WorkLogs.Count;

        protected override string RelatedCollectionLabel => "worklog associati";

        protected override Task OnBeforeUpdateAsync(UnitOfWork uow, Guid id, EmployeeDto.Update dto, Employee entity, CancellationToken ct)
        {
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
            return Task.CompletedTask;
        }
    }
}
