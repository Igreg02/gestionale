using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Interfaces

{
    public interface IDbContextService
    {
        // Sola Lettura
        T ExecuteReadOnly<T>(Func<Session, T> query);
        void ExecuteReadOnly(Action<Session> query);

        // Lettura e Scrittura
        Task ReadWrite(Func<UnitOfWork, Task> operation);
        Task<T> ReadWrite<T>(Func<UnitOfWork, Task<T>> operation);

    }
    }