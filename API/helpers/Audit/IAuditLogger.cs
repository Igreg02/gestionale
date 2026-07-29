namespace GestionaleRendicontazione.Api.Helpers.Audit
{
    public interface IAuditLogger
    {
        void BusinessAction(string action, object? details = null);

        void AuthEvent(string eventName, string userName, bool success);

        void ResourceLifecycle(string eventName, string resourceType, object resourceId);
    }
}
