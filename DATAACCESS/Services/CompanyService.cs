using System.Linq.Expressions;
using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Dataaccess.Services.Abstractions;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class CompanyService
        : XpoCrudServiceBase<Company, CompanyDto.Response, CompanyDto.Create, CompanyDto.Update>,
          ICompanyService
    {
        public CompanyService(IDbContextService db, IMapper mapper) : base(db, mapper) { }

        protected override Company CreateEntity(UnitOfWork uow) => new Company(uow);

        protected override Expression<Func<Company, string>> OrderByExpr => c => c.Name;

        protected override string EntityKindSingular => "l'azienda";

        protected override string EntityLogName(Company entity) => entity.Name;

        protected override int? GetRelatedChildrenCount(Company entity) => entity.Project.Count;

        protected override string RelatedCollectionLabel => "progetti";
    }
}