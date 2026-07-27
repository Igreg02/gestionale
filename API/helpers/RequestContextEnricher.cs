using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace GestionaleRendicontazione.Api.Helpers
{
    // Arricchisce ogni log event con UserId, RequestPath e RequestMethod letti
    // "al volo" da HttpContext, invece di pusharli una volta sola nel LogContext.
    // Questo garantisce che i dati siano presenti anche nei log emessi
    // dall'exception handler (dove un push/pop via 'using' verrebbe già
    // ripulito durante la risalita dell'eccezione, prima di poter loggare).
    public class RequestContextEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RequestContextEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context is null)
            {
                // Log emessi fuori da una richiesta HTTP (avvio app, seeding, background job).
                // Li marchiamo come "system" per distinguerli dalle richieste anonime esterne
                // (che invece ricevono "Anonymous" da ClaimsPrincipalExtensions.GetUserId).
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", "system"));
                return;
            }

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", context.User.GetUserId()));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestPath", context.Request.Path.ToString()));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestMethod", context.Request.Method));
        }
    }
}
