using DevExpress.Xpo;

namespace GestionaleRendicontazione.Domain.Interfaces

{
    public interface IDbContextService
    {
        T ExecuteReadOnly<T>(Func<Session, T> query);
        void ExecuteReadOnly(Action<Session> query);

        Task ReadWriteAsync(Func<UnitOfWork, Task> operation, CancellationToken cancellationToken = default);
        Task<T> ReadWriteAsync<T>(Func<UnitOfWork, Task<T>> operation, CancellationToken cancellationToken = default);
    }
    }