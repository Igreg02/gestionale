using System.Linq.Expressions;
using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Dataaccess.Helpers;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services.Abstractions
{
    public abstract class XpoCrudServiceBase<TEntity, TResponse, TCreate, TUpdate>
        where TEntity : IXPObject
    {
        protected IDbContextService Db { get; }
        protected IMapper Mapper { get; }

        protected XpoCrudServiceBase(IDbContextService db, IMapper mapper)
        {
            Db = db;
            Mapper = mapper;
        }

        protected abstract TEntity CreateEntity(UnitOfWork uow);

        protected abstract Expression<Func<TEntity, string>> OrderByExpr { get; }

        protected abstract string EntityKindSingular { get; }


        protected abstract string EntityLogName(TEntity entity);

        protected virtual int? GetRelatedChildrenCount(TEntity entity) => null;

        protected abstract string RelatedCollectionLabel { get; }

        protected virtual Task OnBeforeCreateAsync(UnitOfWork uow, TCreate dto, TEntity entity, CancellationToken ct)
            => Task.CompletedTask;

        protected virtual Task OnBeforeUpdateAsync(UnitOfWork uow, Guid id, TUpdate dto, TEntity entity, CancellationToken ct)
            => Task.CompletedTask;

        protected virtual bool SupportsCreate => true;

        // ── CRUD ───────────────────────────────────────────────────────────

        public Task<List<TResponse>> GetAllAsync(CancellationToken ct = default)
        {
            return Task.FromResult(Db.ExecuteReadOnly<List<TResponse>>(session =>
            {
                var keySelector = OrderByExpr.Compile();
                var list = session.GetAllOrderedBy<TEntity, string>(keySelector);
                return Mapper.Map<List<TResponse>>(list);
            }));
        }

        public Task<TResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return Task.FromResult(Db.ExecuteReadOnly<TResponse?>(session =>
            {
                var entity = session.GetObjectByKey<TEntity>(id);
                return entity is null ? default : Mapper.Map<TResponse>(entity);
            }));
        }

        public async Task<TResponse> CreateAsync(TCreate dto, CancellationToken ct = default)
        {
            if (!SupportsCreate)
            {
                throw new NotImplementedException(
                    $"CreateAsync non è supportato per {typeof(TEntity).Name}.");
            }

            return await Db.ReadWriteAsync<TResponse>(async uow =>
            {
                var entity = CreateEntity(uow);
                Mapper.Map(dto, entity);
                await OnBeforeCreateAsync(uow, dto, entity, ct);

                return Mapper.Map<TResponse>(entity);
            }, ct);
        }

        public async Task<TResponse?> UpdateAsync(Guid id, TUpdate dto, CancellationToken ct = default)
        {
            return await Db.ReadWriteAsync<TResponse?>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<TEntity>(id, ct);
                if (entity is null) return default;

                Mapper.Map(dto, entity);
                await OnBeforeUpdateAsync(uow, id, dto, entity, ct);

                return Mapper.Map<TResponse>(entity);
            }, ct);
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            return await Db.ReadWriteAsync<bool>(async uow =>
            {
                var entity = await uow.GetObjectByKeyAsync<TEntity>(id, ct);
                if (entity is null) return false;

                var relatedCount = GetRelatedChildrenCount(entity);
                if (relatedCount is > 0)
                {
                    throw new InvalidOperationException(
                        $"Impossibile eliminare {EntityKindSingular} '{EntityLogName(entity)}': " +
                        $"esistono {relatedCount} {RelatedCollectionLabel} collegati.");
                }

                uow.Delete(entity);
                return true;
            }, ct);
        }
    }
}