using System.Linq.Expressions;
using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Dataaccess.Services.Abstractions;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class TypeService
        : XpoCrudServiceBase<Domain.Entities.Type, TypeDto.Response, TypeDto.Create, TypeDto.Update>,
          ITypeService
    {
        public TypeService(IDbContextService db, IMapper mapper) : base(db, mapper) { }

        protected override Domain.Entities.Type CreateEntity(UnitOfWork uow) => new Domain.Entities.Type(uow);

        protected override Expression<Func<Domain.Entities.Type, string>> OrderByExpr => t => t.Name;

        protected override string EntityKindSingular => "il tipo";

        protected override string EntityLogName(Domain.Entities.Type entity) => entity.Name;

        protected override int? GetRelatedChildrenCount(Domain.Entities.Type entity) => entity.WorkLog.Count;

        protected override string RelatedCollectionLabel => "worklog";
    }
}
