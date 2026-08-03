using GestionaleRendicontazione.Api.Helpers;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace GestionaleRendicontazione.Api.Services.Auth
{
    /// <summary>
    /// Marca un'action come accessibile anche quando l'employee autenticato ha
    /// <see cref="Employee.MustChangePassword"/> = true (whitelist del gate sotto).
    /// Usato solo su AuthController.ChangePassword e AuthController.Logout.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AllowPasswordChangeRequiredAttribute : Attribute
    {
    }

    /// <summary>
    /// Blocca ogni richiesta autenticata (tranne quelle marcate
    /// <see cref="AllowPasswordChangeRequiredAttribute"/>) se l'employee ha
    /// MustChangePassword = true. Il controllo è SEMPRE letto live dal DB, non dal
    /// claim JWT: il claim è solo un hint per il frontend (redirect immediato dopo
    /// login), ma è statico per tutta la durata del token — se l'Admin forza il
    /// reset su un utente già loggato, il vecchio token non avrebbe il claim
    /// aggiornato. Il check live qui garantisce che il blocco valga da subito.
    ///
    /// Va registrato DOPO app.UseAuthorization() (gira solo su richieste già
    /// autenticate/autorizzate per ruolo, niente query sprecate su richieste che
    /// falliscono comunque) e PRIMA di app.MapControllers().
    /// </summary>
    public static class PasswordChangeGate
    {
        public const string RequiredHeaderName = "X-Password-Change-Required";

        public static IApplicationBuilder UseMustChangePasswordGate(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    var bypass = context.GetEndpoint()?.Metadata
                        .GetMetadata<AllowPasswordChangeRequiredAttribute>() is not null;

                    if (!bypass)
                    {
                        var employeeId = context.User.GetEmployeeId();
                        if (employeeId is not null)
                        {
                            var db = context.RequestServices.GetRequiredService<IDbContextService>();
                            var mustChangePassword = db.ExecuteReadOnly(session =>
                                session.GetObjectByKey<Employee>(employeeId.Value)?.MustChangePassword ?? false);

                            if (mustChangePassword)
                            {
                                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                                context.Response.Headers[RequiredHeaderName] = "true";
                                context.Response.ContentType = "application/problem+json";
                                await context.Response.WriteAsync(
                                    "{\"title\":\"Cambio password obbligatorio\",\"detail\":\"Devi cambiare la password prima di continuare.\"}");
                                return;
                            }
                        }
                    }
                }

                await next();
            });
        }
    }
}
