using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Exceptions;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
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
                throw new ForeignKeyNotFoundException("Una delle FK fornite non esiste");
            return loaded;
        }

        public MappingProfile()
        {
            // ======================================================================
            // ENTITY -> RESPONSE DTO (lettura: l'istanza esiste già, mapping normale)
            // ======================================================================
            CreateMap<Company, CompanyDto.Response>();
            CreateMap<Employee, EmployeeDto.Response>();
            CreateMap<Project, ProjectDto.Response>()
                .ForMember(dest => dest.IdCompany, opt => opt.MapFrom(src => src.Company != null ? src.Company.Id : Guid.Empty))
                .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty));
            CreateMap<Status, StatusDto.Response>();
            CreateMap<Domain.Entities.Type, TypeDto.Response>();
            CreateMap<LogApplicativo, LogDto.Response>();

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

            // ======================================================================
            // DTO -> ENTITY (scrittura: l'istanza XPO esiste già, NON istanziare)
            //
            // Le entity XPO hanno solo ctor(Session): AutoMapper non può fare new T().
            // Il service crea l'istanza con new T(uow) PRIMA di chiamare Mapper.Map(dto, entity),
            // quindi qui usiamo .ConvertUsing per dire "l'istanza è già pronta, copia solo
            // le proprietà scalari e lascia in pace tutto il resto".
            // ======================================================================

            // ----- Company -----
            CreateMap<CompanyDto.Create, Company>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    if (dest is null) return null!;
                    dest.Name = src.Name;
                    dest.Email = src.Email;
                    return dest;
                });

            CreateMap<CompanyDto.Update, Company>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    if (dest is null) return null!;
                    dest.Name = src.Name;
                    dest.Email = src.Email;
                    return dest;
                });

            // ----- Employee: solo Update (Create passa da AuthService.RegisterAsync) -----
            CreateMap<EmployeeDto.Update, Employee>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    if (dest is null) return null!;
                    // Oid (Id), PasswordHash, Roles, IsActive, MustChangePassword
                    // sono gestiti altrove e NON vanno toccati dal mapper.
                    dest.UserName = src.Username;
                    dest.FirstName = src.FirstName;
                    dest.LastName = src.LastName;
                    return dest;
                });

            // ----- Project (FK Company risolta in OnBeforeCreate/UpdateAsync) -----
            CreateMap<ProjectDto.Create, Project>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    if (dest is null) return null!;
                    dest.Name = src.Name;
                    // Company ignorata: viene impostata nell'hook OnBeforeCreateAsync del service.
                    return dest;
                });

            CreateMap<ProjectDto.Update, Project>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    if (dest is null) return null!;
                    dest.Name = src.Name;
                    return dest;
                });

            // ----- Type -----
            CreateMap<TypeDto.Create, Domain.Entities.Type>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    if (dest is null) return null!;
                    dest.Name = src.Name;
                    return dest;
                });

            CreateMap<TypeDto.Update, Domain.Entities.Type>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    if (dest is null) return null!;
                    dest.Name = src.Name;
                    return dest;
                });

            // ----- Status -----
            CreateMap<StatusDto.Create, Status>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    if (dest is null) return null!;
                    dest.Name = src.Name;
                    return dest;
                });

            CreateMap<StatusDto.Update, Status>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    if (dest is null) return null!;
                    dest.Name = src.Name;
                    return dest;
                });

            // ----- WorkLog Admin -> User: stessi campi scalari, IdEmployee escluso
            // (il DTO User non lo espone: il dipendente è sempre quello autenticato).
            // Usato da WorklogController per costruire il body verso IWorkLogUserService
            // senza duplicare a mano la copia campo-per-campo tra Create e Update.
            CreateMap<WorkLogDto.Admin.Create, WorkLogDto.User.Create>();
            CreateMap<WorkLogDto.Admin.Update, WorkLogDto.User.Update>();

            // ----- WorkLog: campi scalari + risoluzione FK da UoW -----
            // Tutta la logica (campi + FK + timestamp) vive dentro ConvertUsing.

            CreateMap<WorkLogDto.Admin.Create, WorkLog>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    if (dest is null) return null!;
                    var now = DateTime.UtcNow;
                    dest.Description = src.Description;
                    dest.HoursCounter = src.HoursCounter;
                    dest.Date = src.Date;
                    dest.Project = LoadXpo<Project>(ctx, src.IdProject);
                    dest.Type = LoadXpo<Domain.Entities.Type>(ctx, src.IdType);
                    dest.Status = LoadXpo<Status>(ctx, src.IdStatus);
                    dest.Employee = LoadXpo<Employee>(ctx, src.IdEmployee);
                    dest.CreateAt = now;
                    dest.UpdateAt = now;
                    dest.IsWorkLogDeleted = false;
                    return dest;
                });

            CreateMap<WorkLogDto.Admin.Update, WorkLog>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    if (dest is null) return null!;
                    dest.Description = src.Description;
                    dest.HoursCounter = src.HoursCounter;
                    dest.Date = src.Date;
                    dest.Project = LoadXpo<Project>(ctx, src.IdProject);
                    dest.Type = LoadXpo<Domain.Entities.Type>(ctx, src.IdType);
                    dest.Status = LoadXpo<Status>(ctx, src.IdStatus);
                    dest.Employee = LoadXpo<Employee>(ctx, src.IdEmployee);
                    dest.UpdateAt = DateTime.UtcNow;
                    return dest;
                });

            CreateMap<WorkLogDto.User.Create, WorkLog>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    if (dest is null) return null!;
                    var now = DateTime.UtcNow;
                    dest.Description = src.Description;
                    dest.HoursCounter = src.HoursCounter;
                    dest.Date = src.Date;
                    dest.Project = LoadXpo<Project>(ctx, src.IdProject);
                    dest.Type = LoadXpo<Domain.Entities.Type>(ctx, src.IdType);
                    dest.Status = LoadXpo<Status>(ctx, src.IdStatus);
                    dest.Employee = (Employee)ctx.Items[WorkLogMappingContextKeys.CurrentEmployee];
                    dest.CreateAt = now;
                    dest.UpdateAt = now;
                    dest.IsWorkLogDeleted = false;
                    return dest;
                });

            CreateMap<WorkLogDto.User.Update, WorkLog>()
                .ConvertUsing((src, dest, ctx) =>
                {
                    if (dest is null) return null!;
                    dest.Description = src.Description;
                    dest.HoursCounter = src.HoursCounter;
                    dest.Date = src.Date;
                    dest.Project = LoadXpo<Project>(ctx, src.IdProject);
                    dest.Type = LoadXpo<Domain.Entities.Type>(ctx, src.IdType);
                    dest.Status = LoadXpo<Status>(ctx, src.IdStatus);
                    // Employee invariato in Update
                    dest.UpdateAt = DateTime.UtcNow;
                    return dest;
                });
        }
    }
}
