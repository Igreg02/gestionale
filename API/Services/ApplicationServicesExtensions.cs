using AutoMapper;
using GestionaleRendicontazione.Api.Helpers.Audit;
using GestionaleRendicontazione.Api.Services.Jwt;
using GestionaleRendicontazione.Dataaccess.Helpers;
using GestionaleRendicontazione.Dataaccess.Services;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GestionaleRendicontazione.Api.Services
{
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddControllers();

            services.AddAuthorization();

            services.AddSingleton<PasswordHasher<Employee>>();
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IWorkLogAdminService, WorkLogAdminService>();
            services.AddScoped<IWorkLogUserService, WorkLogUserService>();

            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IStatusService, StatusService>();
            services.AddScoped<ITypeService, TypeService>();

            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ILogService, LogService>();

            services.AddSingleton<IAuditLogger, SerilogAuditLogger>();

            return services;
        }
    }
}
