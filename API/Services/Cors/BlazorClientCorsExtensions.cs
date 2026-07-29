using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionaleRendicontazione.Api.Services.Cors
{
    /// <summary>
    /// CORS — necessario a partire dalla Fase F1 del frontend Blazor WebAssembly,
    /// che gira su un'origine diversa (es. https://localhost:7210) da quella
    /// dell'API. Le origini consentite sono in appsettings.json ("Cors:AllowedOrigins"),
    /// così da poter differenziare sviluppo/produzione senza toccare il codice.
    /// </summary>
    public static class BlazorClientCorsExtensions
    {
        public const string PolicyName = "BlazorClient";

        public static IServiceCollection AddBlazorClientCors(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? Array.Empty<string>();

            services.AddCors(options =>
            {
                options.AddPolicy(PolicyName, policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            return services;
        }
    }
}
