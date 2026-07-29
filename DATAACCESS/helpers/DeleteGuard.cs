using System;
using System.Collections.Generic;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
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