using System.Linq.Expressions;
using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Dataaccess.Services.Abstractions;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class StatusService
        : XpoCrudServiceBase<Status, StatusDto.Response, StatusDto.Create, StatusDto.Update>,
          IStatusService
    {
        public StatusService(IDbContextService db, IMapper mapper) : base(db, mapper) { }

        protected override Status CreateEntity(UnitOfWork uow) => new Status(uow);

        protected override Expression<Func<Status, string>> OrderByExpr => s => s.Name;

        protected override string EntityKindSingular => "lo stato";

        protected override string EntityLogName(Status entity) => entity.Name;

        protected override int? GetRelatedChildrenCount(Status entity) => entity.WorkLog.Count;

        protected override string RelatedCollectionLabel => "worklog";
    }
}
