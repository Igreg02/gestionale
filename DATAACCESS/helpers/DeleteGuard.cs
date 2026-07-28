using System.Collections.Generic;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
    /// <summary>
    /// Lancia <see cref="InvalidOperationException"/> se la collezione di figli ha elementi,
    /// con un messaggio uniforme che descrive l'entità radice e il numero di figli.
    /// Centralizzato perché tutte le Delete dei service CRUD usano la stessa frase.
    /// </summary>
    public static class DeleteGuard
    {
        public static void ThrowIfHasRelated<T>(
            ICollection<T>? children,
            string entityLabel,
            string entityName)
        {
            if (children is { Count: > 0 })
            {
                throw new InvalidOperationException(
                    $"Impossibile eliminare {entityLabel} '{entityName}': esistono {children.Count} elementi collegati.");
            }
        }
    }
}