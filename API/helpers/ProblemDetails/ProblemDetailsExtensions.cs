using System;
using System.Threading.Tasks;
using GestionaleRendicontazione.Domain.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GestionaleRendicontazione.Api.Helpers.ProblemDetails
{
    /// <summary>
    /// Middleware globale per la gestione delle eccezioni non gestite.
    /// Le eccezioni note vengono mappate a uno status HTTP sensato
    /// (InvalidOperationException → 409, UnauthorizedAccessException → 401, …)
    /// e il payload segue il formato RFC 7807 (application/problem+json).
    /// In Development aggiunge traceId + stackTrace.
    /// </summary>
    public static class ProblemDetailsExtensions
    {
        public static IApplicationBuilder UseGlobalProblemDetails(this IApplicationBuilder app)
        {
            return app.UseExceptionHandler(handlerApp =>
            {
                handlerApp.Run(async context =>
                {
                    var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                    var exception = exceptionFeature?.Error;

                    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                    if (exception is not null)
                    {
                        logger.LogError(exception, "Unhandled exception during {Method} {Path}",
                            context.Request.Method, context.Request.Path);
                    }

                    var (status, title) = exception switch
                    {
                        ArgumentException => (StatusCodes.Status400BadRequest, "Richiesta non valida"),
                        ForeignKeyNotFoundException => (StatusCodes.Status422UnprocessableEntity, "Riferimento non valido"),
                        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Non autorizzato"),
                        KeyNotFoundException => (StatusCodes.Status404NotFound, "Risorsa non trovata"),
                        InvalidOperationException => (StatusCodes.Status409Conflict, "Operazione non valida"),
                        NotImplementedException => (StatusCodes.Status501NotImplemented, "Funzionalità non implementata"),
                        TimeoutException => (StatusCodes.Status504GatewayTimeout, "Timeout del server"),
                        OperationCanceledException => (499 /* Client Closed Request, conventione nginx */, "Richiesta annullata dal client"),
                        _ => (StatusCodes.Status500InternalServerError, "Errore interno del server")
                    };

                    context.Response.StatusCode = status;
                    context.Response.ContentType = "application/problem+json";

                    var problem = new Microsoft.AspNetCore.Mvc.ProblemDetails
                    {
                        Status = status,
                        Title = title,
                        Type = $"https://httpstatuses.io/{status}",
                        Detail = exception?.Message,
                        Instance = context.Request.Path
                    };

                    var env = context.RequestServices.GetRequiredService<IHostEnvironment>();
                    if (env.IsDevelopment())
                    {
                        problem.Extensions["traceId"] = context.TraceIdentifier;
                        problem.Extensions["stackTrace"] = exception?.StackTrace;
                    }

                    await context.Response.WriteAsJsonAsync(problem);
                });
            });
        }
    }
}
