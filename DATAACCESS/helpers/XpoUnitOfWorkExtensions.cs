using System;
using System.Threading;
using System.Threading.Tasks;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Exceptions;

namespace GestionaleRendicontazione.Dataaccess.Helpers
{
    /// <summary>
    /// Estensioni su <see cref="UnitOfWork"/> per le operazioni di lookup più comuni nei service.
    /// </summary>
    public static class XpoUnitOfWorkExtensions
    {
        /// <summary>
        /// Come <see cref="UnitOfWork.GetObjectByKeyAsync{T}(object, CancellationToken)"/>,
        /// ma lancia <see cref="ForeignKeyNotFoundException"/> con il messaggio passato dal
        /// chiamante se la foreign key non esiste. Il messaggio arriva al client tramite
        /// ProblemDetails.Detail, quindi è già il wording user-facing.
        /// </summary>
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
                throw new ForeignKeyNotFoundException(messageWhenMissing);
            }
            return entity;
        }
    }
}