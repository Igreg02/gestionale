using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Interfaces

{
    public interface IDbContextService
    {
        // Sola Lettura
        T ExecuteReadOnly<T>(Func<Session, T> query);
        void ExecuteReadOnly(Action<Session> query);

        // Lettura e Scrittura
        Task ReadWriteAsync(Func<UnitOfWork, Task> operation, CancellationToken cancellationToken = default);
        Task<T> ReadWriteAsync<T>(Func<UnitOfWork, Task<T>> operation, CancellationToken cancellationToken = default);
    }
    }