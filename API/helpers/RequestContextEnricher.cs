using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace GestionaleRendicontazione.Api.Helpers
{
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
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", "system"));
                return;
            }

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", context.User.GetUserId()));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestPath", context.Request.Path.ToString()));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("RequestMethod", context.Request.Method));
        }
    }
}
