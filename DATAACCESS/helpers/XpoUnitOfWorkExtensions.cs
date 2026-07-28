using System.Threading;
using System.Threading.Tasks;
using DevExpress.Xpo;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
    /// <summary>
    /// Estensioni su <see cref="UnitOfWork"/> per le operazioni di lookup più comuni nei service.
    /// </summary>
    public static class XpoUnitOfWorkExtensions
    {
        /// <summary>
        /// Come <see cref="UnitOfWork.GetObjectByKeyAsync{T}(object, CancellationToken)"/>,
        /// ma lancia <see cref="InvalidOperationException"/> con un messaggio descrittivo
        /// se la foreign key non esiste.
        /// </summary>
        public static async Task<T> GetRequiredObjectByKeyAsync<T>(
            this UnitOfWork uow,
            object key,
            string entityLabel,
            CancellationToken ct = default)
            where T : class
        {
            var entity = await uow.GetObjectByKeyAsync<T>(key, ct);
            if (entity is null)
            {
                throw new InvalidOperationException($"{entityLabel} con Id '{key}' non trovata.");
            }
            return entity;
        }
    }
}