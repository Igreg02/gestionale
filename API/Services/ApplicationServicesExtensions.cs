using AutoMapper;
using GestionaleRendicontazione.Api.Services.Jwt;
using GestionaleRendicontazione.Dataaccess.Helpers;
using GestionaleRendicontazione.Dataaccess.Services;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace GestionaleRendicontazione.Api.Services
{
    /// <summary>
    /// Registra tutto ciò che è "business" lato API:
    /// - infrastructure trasversale (Authorization, PasswordHasher, AutoMapper);
    /// - servizi di autenticazione/gestione (token, blacklist, worklog, anagrafiche, report, log).
    /// </summary>
    public static class ApplicationServicesExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Autorizzazione (le policy puntuali sono AddAuthorization(...) altrove se ne servono)
            services.AddAuthorization();

            // Hashing password e mapping DTO ↔ entity
            services.AddSingleton<PasswordHasher<Employee>>();
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            // Autenticazione / token
            services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();
            services.AddSingleton<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IAuthService, AuthService>();

            // Worklog
            services.AddScoped<IWorkLogAdminService, WorkLogAdminService>();
            services.AddScoped<IWorkLogUserService, WorkLogUserService>();

            // Anagrafiche
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IStatusService, StatusService>();
            services.AddScoped<ITypeService, TypeService>();

            // Report & log applicativo
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<ILogService, LogService>();

            return services;
        }
    }
}
