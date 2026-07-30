using System.Linq.Expressions;
using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Dataaccess.Helpers;
using GestionaleRendicontazione.Dataaccess.Services.Abstractions;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    
    public class ProjectService
        : XpoCrudServiceBase<Project, ProjectDto.Response, ProjectDto.Create, ProjectDto.Update>,
          IProjectService
    {
        public ProjectService(IDbContextService db, IMapper mapper) : base(db, mapper) { }

        protected override Project CreateEntity(UnitOfWork uow) => new Project(uow);

        protected override Expression<Func<Project, string>> OrderByExpr => p => p.Name;

        protected override string EntityKindSingular => "il progetto";

        protected override string EntityLogName(Project entity) => entity.Name;

        protected override int? GetRelatedChildrenCount(Project entity) => entity.WorkLog.Count;

        protected override string RelatedCollectionLabel => "worklog";

        protected override Task OnBeforeCreateAsync(UnitOfWork uow, ProjectDto.Create dto, Project entity, CancellationToken ct)
        {
            // Risolve la FK Company prima del commit. Il mapper ha già popolato
            // Name (scalare); Company è Ignored nel mapping perché richiede la UoW.
            // Cf. MappingProfile.cs: CreateMap<ProjectDto.Create, Project>().
            return ResolveCompanyAsync(uow, dto.IdCompany, entity, ct);
        }

        protected override Task OnBeforeUpdateAsync(UnitOfWork uow, Guid id, ProjectDto.Update dto, Project entity, CancellationToken ct)
        {
            return ResolveCompanyAsync(uow, dto.IdCompany, entity, ct);
        }

        private static async Task ResolveCompanyAsync(UnitOfWork uow, Guid idCompany, Project entity, CancellationToken ct)
        {
            entity.Company = await uow.GetRequiredAsync<Company>(idCompany, "Azienda non trovata", ct);
        }
    }
}
