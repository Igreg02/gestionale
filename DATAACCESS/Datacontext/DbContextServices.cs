using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Datacontext.DbContextService
{

    public class DbContextService : IDbContextService
    {
        private readonly IDataLayer _dataLayer;

        public DbContextService(IDataLayer dataLayer)
        {
            _dataLayer = dataLayer;
        }

        public T ExecuteReadOnly<T>(Func<Session, T> query)
        {
            using (var session = new Session(_dataLayer))
            {
                session.TrackPropertiesModifications = false;

                return query(session);
            }
        }

        public void ExecuteReadOnly(Action<Session> query)
        {
            using (var session = new Session(_dataLayer))
            {
                session.TrackPropertiesModifications = false;

                query(session);
            }
        }

        public async Task ReadWriteAsync(Func<UnitOfWork, Task> operation, CancellationToken cancellationToken = default)
        {
            using (var uow = new UnitOfWork(_dataLayer))
            {
                await operation(uow);
                await uow.CommitChangesAsync(cancellationToken);
            }
        }

        public async Task<T> ReadWriteAsync<T>(Func<UnitOfWork, Task<T>> operation, CancellationToken cancellationToken = default)
        {
            using (var uow = new UnitOfWork(_dataLayer))
            {
                var result = await operation(uow);
                await uow.CommitChangesAsync(cancellationToken);
                return result;
            }
        }
    }
}