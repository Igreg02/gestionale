using System;
using System.Threading.Tasks;
using DevExpress.Xpo;

namespace GestionaleRendicontazione.Infrastructure.Data
{
    public interface IXpoContextService
    {
        // Sola Lettura
        T ExecuteReadOnly<T>(Func<Session, T> query);
        void ExecuteReadOnly(Action<Session> query);

        // Lettura e Scrittura
        Task ExecuteTransactionAsync(Func<UnitOfWork, Task> operation);
        void ClearCache();
    }

    public class XpoContextService : IXpoContextService
    {
        private readonly IDataLayer _dataLayer;

        // Il DataLayer viene iniettato ed è unico per l'applicazione (Singleton)
        public XpoContextService(IDataLayer dataLayer)
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
        public async Task ExecuteTransactionAsync(Func<UnitOfWork, Task> operation)
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
                    // RIMUOVERE (CONTROLLARE)In caso di errore XPO fa automaticamente il Rollback della transazione.
                    throw;
                }
            }
        }

        // =======================
        // 3. METODO PULIZIA CACHE (TEORICA PULIZIA AUTOMATICA)
        // =======================
        public void ClearCache()
        {/*
            using (var session = new Session(_dataLayer))
            {
                session.DropIdentityMap();
            }
        */
        }
    }
}