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
                session.TrackPropertiesModifications = false; // Risparmio risorse è in readonly

                return query(session);
            }
        }

        public void ExecuteReadOnly(Action<Session> query)
        {
            using (var session = new Session(_dataLayer))
            {
                session.TrackPropertiesModifications = false; // Risparmio risorse è in readonly

                query(session);
            }
        }


        public async Task ReadWrite(Func<UnitOfWork, Task> operation)
        {
            using (var uow = new UnitOfWork(_dataLayer))
            {
                try
                {
                    await operation(uow);
                    await uow.CommitChangesAsync();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
                public async Task<T> ReadWrite<T>(Func<UnitOfWork, Task<T>> operation)
        {
            using (var uow = new UnitOfWork(_dataLayer))
            {
                try
                {
                    var result = await operation(uow);
                    await uow.CommitChangesAsync();
                    return result;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}