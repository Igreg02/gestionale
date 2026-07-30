using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services.Abstractions
{
    /// <summary>
    /// Variante di <see cref="XpoCrudServiceBase{TEntity,TResponse,TCreate,TUpdate}"/>
    /// per le entity la cui creazione NON passa dal CRUD standard (es. <c>Employee</c>,
    /// creata solo via <c>AuthService.RegisterAsync</c>).
    ///
    /// Forza <see cref="XpoCrudServiceBase{TEntity,TResponse,TCreate,TUpdate}.SupportsCreate"/>
    /// a <c>false</c> e usa lo stesso tipo per Create/Update, evitando di dichiarare
    /// <c>TCreate = TUpdate</c> due volte nella firma del service derivato.
    /// </summary>
    public abstract class XpoCrudServiceBaseNoCreate<TEntity, TResponse, TUpdate>
        : XpoCrudServiceBase<TEntity, TResponse, TUpdate, TUpdate>
        where TEntity : IXPObject
    {
        protected XpoCrudServiceBaseNoCreate(IDbContextService db, IMapper mapper) : base(db, mapper) { }

        protected override bool SupportsCreate => false;
    }
}
