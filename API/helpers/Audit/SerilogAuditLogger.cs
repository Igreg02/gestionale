using Serilog;

namespace GestionaleRendicontazione.Api.Helpers.Audit
{
    public sealed class SerilogAuditLogger : IAuditLogger
    {
        public void BusinessAction(string action, object? details = null)
        {
            if (details is null)
            {
                Log.Information("AUDIT: {Action}", action);
            }
            else
            {
                Log.Information("AUDIT: {Action} {Details}", action, details);
            }
        }

        public void AuthEvent(string eventName, string userName, bool success)
        {
            var outcome = success ? "succeeded" : "failed";
            var template = "AUTH: {Event} {Outcome} for userName={UserName}";

            if (success)
            {
                Log.Information(template, eventName, outcome, userName);
            }
            else
            {
                Log.Warning(template, eventName, outcome, userName);
            }
        }

        public void ResourceLifecycle(string eventName, string resourceType, object resourceId)
        {
            Log.Information("RESOURCE: {Event} {ResourceType} {ResourceId}", eventName, resourceType, resourceId);
        }
    }
}
