using System;
using System.Collections.Generic;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
    /// <summary>
    /// Lancia <see cref="InvalidOperationException"/> con il messaggio passato dal chiamante
    /// se la collezione di figli ha elementi. Il messaggio arriva al client tramite
    /// ProblemDetails.Detail, quindi è già il wording user-facing.
    /// </summary>
    public static class DeleteGuard
    {
        public static void ThrowIfHasRelated<T>(
            ICollection<T>? children,
            string messageWhenBlocked)
        {
            if (children is { Count: > 0 })
            {
                throw new InvalidOperationException(messageWhenBlocked);
            }
        }
    }
}