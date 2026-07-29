using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace GestionaleRendicontazione.Api.Services.Swagger
{
    /// <summary>
    /// Configurazione di Swagger/OpenAPI per l'API:
    /// schema id "fullname" (per non collidere tra DTO/domain con lo stesso nome),
    /// e definizione di sicurezza "Bearer" che permette al client di incollare
    /// solo il token JWT (il prefisso "Bearer " viene aggiunto in automatico).
    /// </summary>
    public static class SwaggerExtensions
    {
        public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "Inserisci unicamente il token JWT (il prefisso 'Bearer ' verrà aggiunto in automatico).",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
                });
            });
            return services;
        }
    }
}
