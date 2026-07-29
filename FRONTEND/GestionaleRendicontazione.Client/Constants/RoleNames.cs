namespace GestionaleRendicontazione.Client.Constants
{
    /// <summary>
    /// Specchio client-side di GestionaleRendicontazione.Domain.Constants.RoleNames (backend).
    /// Il client Blazor WASM è standalone e non referenzia il progetto Domain: i due elenchi vanno
    /// mantenuti allineati manualmente finché non si introduce una generazione condivisa dei contratti
    /// (vedi TDD §7, Fase F3 — "Convenzione di progetto per i DTO condivisi con il backend").
    /// </summary>
    public static class RoleNames
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
}
