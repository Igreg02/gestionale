using System;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Xpo;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
    public static class XpoUnitOfWorkExtensions
    {
        public static async Task<T> GetRequiredAsync<T>(
            this UnitOfWork uow,
            object key,
            string messageWhenMissing,
            CancellationToken ct = default)
            where T : class
        {
            var entity = await uow.GetObjectByKeyAsync<T>(key, ct);
            if (entity is null)
            {
                throw new InvalidOperationException(messageWhenMissing);
            }
            return entity;
        }
    }
}