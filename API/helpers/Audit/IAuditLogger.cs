namespace GestionaleRendicontazione.Api.Helpers.Audit
{
    /// <summary>
    /// Punto unico di emissione dei log di business (audit). Centralizza i
    /// messaggi con prefissi e livelli coerenti, così i record che arrivano
    /// nel DB tramite XpoSerilogSink restano leggibili e filtrabili.
    ///
    /// UserId / RequestPath / RequestMethod NON vanno passati qui: li aggiunge
    /// automaticamente RequestContextEnricher su ogni LogEvent.
    /// </summary>
    public interface IAuditLogger
    {
        /// <summary>
        /// Azione di business generica (es. "Esportazione report", "Import anagrafica").
        /// Prefisso messaggio: "AUDIT: ". Livello: Information.
        /// </summary>
        void BusinessAction(string action, object? details = null);

        /// <summary>
        /// Evento di autenticazione (Login, Logout, Register, ...).
        /// Prefisso messaggio: "AUTH: ". Livello: Information se success=true, Warning altrimenti.
        /// </summary>
        void AuthEvent(string eventName, string userName, bool success);

        /// <summary>
        /// Ciclo di vita di una risorsa (Created, Updated, Deleted).
        /// Prefisso messaggio: "RESOURCE: ". Livello: Information.
        /// </summary>
        void ResourceLifecycle(string eventName, string resourceType, object resourceId);
    }
}
