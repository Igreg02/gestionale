using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
    /// <summary>
    /// Chiavi convenzionali usate in <c>MappingOptions.Items</c>. Il service vi
    /// inserisce l'oggetto XPO di sessione (UnitOfWork) e - per il solo User.Create -
    /// il dipendente autenticato, prima di invocare il mapper.
    /// </summary>
    public static class WorkLogMappingContextKeys
    {
        public const string Uow = "Uow";
        public const string CurrentEmployee = "CurrentEmployee";
    }

    public class MappingProfile : Profile
    {
        // Helper: carica un oggetto XPO dalla UnitOfWork presente in ctx.Items.
        private static TForeign LoadXpo<TForeign>(ResolutionContext ctx, Guid id)
            where TForeign : class
        {
            var uow = (UnitOfWork)ctx.Items[WorkLogMappingContextKeys.Uow];
            var loaded = (TForeign)uow.GetObjectByKey(typeof(TForeign), id);
            if (loaded is null)
                throw new InvalidOperationException("Una delle FK fornite non esiste");
            return loaded;
        }

        public MappingProfile()
        {
            // Company
            CreateMap<Company, CompanyDto.Response>();

            // Employee
            CreateMap<Employee, EmployeeDto.Response>();

            // Project
            CreateMap<Project, ProjectDto.Response>()
                .ForMember(dest => dest.IdCompany, opt => opt.MapFrom(src => src.Company != null ? src.Company.Id : Guid.Empty))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty));

            // Status
            CreateMap<Status, StatusDto.Response>();

            // Type
            CreateMap<Domain.Entities.Type, TypeDto.Response>();

            // LogApplicativo: mappato solo in lettura, escludendo StackTrace dal payload
            CreateMap<LogApplicativo, LogDto.Response>();

            // WorkLog
            CreateMap<WorkLog, WorkLogDto.Admin.Response>()
                .ForMember(dest => dest.IdProject, opt => opt.MapFrom(src => src.Project != null ? src.Project.Id : Guid.Empty))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : string.Empty))
                .ForMember(dest => dest.IdEmployee, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.Id : Guid.Empty))
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.UserName : string.Empty))
                .ForMember(dest => dest.IdType, opt => opt.MapFrom(src => src.Type != null ? src.Type.Id : Guid.Empty))
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type != null ? src.Type.Name : string.Empty))
                .ForMember(dest => dest.IdStatus, opt => opt.MapFrom(src => src.Status != null ? src.Status.Id : Guid.Empty))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status != null ? src.Status.Name : string.Empty));

            CreateMap<WorkLog, WorkLogDto.User.Response>()
                .ForMember(dest => dest.IdProject, opt => opt.MapFrom(src => src.Project != null ? src.Project.Id : Guid.Empty))
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : string.Empty))
                .ForMember(dest => dest.IdType, opt => opt.MapFrom(src => src.Type != null ? src.Type.Id : Guid.Empty))
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src => src.Type != null ? src.Type.Name : string.Empty))
                .ForMember(dest => dest.IdStatus, opt => opt.MapFrom(src => src.Status != null ? src.Status.Id : Guid.Empty))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Status != null ? src.Status.Name : string.Empty));

            // DTO -> Entity mapping (write/update)
            CreateMap<CompanyDto.Create, Company>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore());

            CreateMap<CompanyDto.Update, Company>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore());

            CreateMap<EmployeeDto.Update, Employee>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Oid, opt => opt.Ignore())
                .ForMember(dest => dest.WorkLogs, opt => opt.Ignore());

            // WorkLog — DTO -> Entity (write/update).
            // Il mapper copia gli scalari (Description, HoursCounter, Date) per
            // convenzione; FK, timestamp e flag soft-delete sono risolti in AfterMap
            // prelevando la UnitOfWork da ctx.Items.
            // Update: CreateAt/IsWorkLogDeleted restano sull'entity caricata.

            // ----- Admin.Create -----
            CreateMap<WorkLogDto.Admin.Create, WorkLog>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreateAt, o => o.Ignore())
                .ForMember(d => d.UpdateAt, o => o.Ignore())
                .ForMember(d => d.IsWorkLogDeleted, o => o.Ignore())
                .ForMember(d => d.Project, o => o.Ignore())
                .ForMember(d => d.Type, o => o.Ignore())
                .ForMember(d => d.Status, o => o.Ignore())
                .ForMember(d => d.Employee, o => o.Ignore())
                .AfterMap((src, dest, ctx) =>
                {
                    var now = DateTime.UtcNow;
                    dest.Project = LoadXpo<Project>(ctx, src.IdProject);
                    dest.Type = LoadXpo<Domain.Entities.Type>(ctx, src.IdType);
                    dest.Status = LoadXpo<Status>(ctx, src.IdStatus);
                    dest.Employee = LoadXpo<Employee>(ctx, src.IdEmployee);
                    dest.CreateAt = now;
                    dest.UpdateAt = now;
                    dest.IsWorkLogDeleted = false;
                });

            // ----- Admin.Update -----
            CreateMap<WorkLogDto.Admin.Update, WorkLog>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreateAt, o => o.Ignore())
                .ForMember(d => d.IsWorkLogDeleted, o => o.Ignore())
                .ForMember(d => d.Project, o => o.Ignore())
                .ForMember(d => d.Type, o => o.Ignore())
                .ForMember(d => d.Status, o => o.Ignore())
                .ForMember(d => d.Employee, o => o.Ignore())
                .AfterMap((src, dest, ctx) =>
                {
                    dest.Project = LoadXpo<Project>(ctx, src.IdProject);
                    dest.Type = LoadXpo<Domain.Entities.Type>(ctx, src.IdType);
                    dest.Status = LoadXpo<Status>(ctx, src.IdStatus);
                    dest.Employee = LoadXpo<Employee>(ctx, src.IdEmployee);
                    dest.UpdateAt = DateTime.UtcNow;
                });

            // ----- User.Create: Employee dal dipendente autenticato (ctx.Items) -----
            CreateMap<WorkLogDto.User.Create, WorkLog>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreateAt, o => o.Ignore())
                .ForMember(d => d.UpdateAt, o => o.Ignore())
                .ForMember(d => d.IsWorkLogDeleted, o => o.Ignore())
                .ForMember(d => d.Project, o => o.Ignore())
                .ForMember(d => d.Type, o => o.Ignore())
                .ForMember(d => d.Status, o => o.Ignore())
                .ForMember(d => d.Employee, o => o.Ignore())
                .AfterMap((src, dest, ctx) =>
                {
                    var now = DateTime.UtcNow;
                    dest.Project = LoadXpo<Project>(ctx, src.IdProject);
                    dest.Type = LoadXpo<Domain.Entities.Type>(ctx, src.IdType);
                    dest.Status = LoadXpo<Status>(ctx, src.IdStatus);
                    dest.Employee = (Employee)ctx.Items[WorkLogMappingContextKeys.CurrentEmployee];
                    dest.CreateAt = now;
                    dest.UpdateAt = now;
                    dest.IsWorkLogDeleted = false;
                });

            // ----- User.Update: Employee invariato -----
            CreateMap<WorkLogDto.User.Update, WorkLog>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreateAt, o => o.Ignore())
                .ForMember(d => d.IsWorkLogDeleted, o => o.Ignore())
                .ForMember(d => d.Project, o => o.Ignore())
                .ForMember(d => d.Type, o => o.Ignore())
                .ForMember(d => d.Status, o => o.Ignore())
                .ForMember(d => d.Employee, o => o.Ignore())
                .AfterMap((src, dest, ctx) =>
                {
                    dest.Project = LoadXpo<Project>(ctx, src.IdProject);
                    dest.Type = LoadXpo<Domain.Entities.Type>(ctx, src.IdType);
                    dest.Status = LoadXpo<Status>(ctx, src.IdStatus);
                    dest.UpdateAt = DateTime.UtcNow;
                });
        }
    }
}
