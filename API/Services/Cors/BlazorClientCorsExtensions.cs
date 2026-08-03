using System;
using GestionaleRendicontazione.Api.Services.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GestionaleRendicontazione.Api.Services.Cors
{
    /// <summary>
    /// CORS — necessario per far  partire Blazor WebAssembly,
    /// che gira su un'origine diversa (es. https://localhost:7210) da quella
    /// dell'API.
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
                          .AllowAnyMethod()
                          // Senza questo, il browser non lascia leggere l'header custom al
                          // codice JS/Blazor lato client anche se il server lo invia — le
                          // richieste cross-origin esporgono di default solo un set ristretto
                          // di header "safe" (vedi AuthenticatedHttpMessageHandler.cs lato client).
                          .WithExposedHeaders(PasswordChangeGate.RequiredHeaderName);
                });
            });

            return services;
        }
    }
}
