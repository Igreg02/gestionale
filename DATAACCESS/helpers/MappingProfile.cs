using AutoMapper;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
    public class MappingProfile : Profile
    {
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
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.email))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore());

            CreateMap<CompanyDto.Update, Company>()
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.email))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Project, opt => opt.Ignore());

            CreateMap<EmployeeDto.Update, Employee>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username))
                .ForMember(dest => dest.Oid, opt => opt.Ignore())
                .ForMember(dest => dest.WorkLogs, opt => opt.Ignore());
        }
    }
}
