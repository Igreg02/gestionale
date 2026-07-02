using System;
using System.Threading.Tasks;
using DevExpress.Xpo;

namespace GestionaleRendicontazione.Dataaccess.Datacontext.DbContextService
{
    public interface IDbContextService
    {
        // Sola Lettura
        T ExecuteReadOnly<T>(Func<Session, T> query);
        void ExecuteReadOnly(Action<Session> query);

        // Lettura e Scrittura
        Task ReadWrite(Func<UnitOfWork, Task> operation);
    }

    public class DbContextService : IDbContextService
    {
        private readonly IDataLayer _dataLayer;

        // Il DataLayer viene iniettato ed è unico per l'applicazione (Singleton)
        public DbContextService(IDataLayer dataLayer)
        {
            _dataLayer = dataLayer;
        }

        // ===============
        // 1. SOLA LETTURA
        // ===============
        public T ExecuteReadOnly<T>(Func<Session, T> query)
        {
            using (var session = new Session(_dataLayer))
            {
                // Disabilita il tracciamento delle modifiche perchè verra utilizzato come ReadOnly
                session.TrackPropertiesModifications = false;

                return query(session);
            }
        }

        public void ExecuteReadOnly(Action<Session> query)
        {
            using (var session = new Session(_dataLayer))
            {
                // Disabilita il tracciamento delle modifiche perchè verra utilizzato come ReadOnly
                session.TrackPropertiesModifications = false;

                query(session);
            }
        }

        // ==========================
        // 2. ASYNC LETTURA/SCRITTURA
        // ==========================
        public async Task ReadWrite(Func<UnitOfWork, Task> operation)
        {
            using (var uow = new UnitOfWork(_dataLayer))
            {
                try
                {
                    // Esegue l'operazione passata tramite Lambda (es: inserimento, update)
                    await operation(uow);
                    // Salva le modifiche nel database in modo asincrono
                    await uow.CommitChangesAsync();
                }
                catch (Exception)
                {
                    // futuro log errore
                    throw;
                }
            }
        }

        // =======================
        // 3. METODO PULIZIA CACHE (TEORICA PULIZIA AUTOMATICA)
        // =======================

    }
}