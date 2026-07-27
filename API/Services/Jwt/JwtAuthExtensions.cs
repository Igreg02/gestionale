using System;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GestionaleRendicontazione.Domain.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace GestionaleRendicontazione.Api.Services.Jwt
{
    /// <summary>
    /// Configura l'autenticazione JWT:
    /// - validazione issuer/audience/lifetime/signing key
    /// - clock skew a zero (i token scaduti sono rifiutati subito)
    /// - claim NameIdentifier come NameClaimType (per User.GetUserId())
    /// - hook OnTokenValidated: scarta i token presenti nella blacklist
    ///   usando l'jti come chiave, in modo che il logout funzioni davvero.
    /// </summary>
    public static class JwtAuthExtensions
    {
        public static IServiceCollection AddJwtAuthenticationWithBlacklist(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var jwtSection = configuration.GetSection("Jwt");
            services.Configure<JwtOptions>(jwtSection);

            var secretKey = jwtSection["SecretKey"] ?? string.Empty;
            var issuer = jwtSection["Issuer"] ?? string.Empty;
            var audience = jwtSection["Audience"] ?? string.Empty;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = true;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = OnTokenValidatedAsync
                    };
                });

            services.AddAuthorization();
            return services;
        }

        private static async Task OnTokenValidatedAsync(TokenValidatedContext context)
        {
            var blacklist = context.HttpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
            var jti = context.Principal?.FindFirst("jti")?.Value;
            if (jti is not null && await blacklist.IsBlacklistedAsync(jti))
            {
                context.Fail("Token revocato.");
            }
        }
    }
}
