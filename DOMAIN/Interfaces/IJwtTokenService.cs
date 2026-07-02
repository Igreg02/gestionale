using System.Security.Claims;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    /// <summary>
    /// Servizio di emissione token JWT self-issued. Implementato nell'API layer (JwtTokenService).
    /// Dichiarato in Domain per rispettare la regola di Dependency Inversion e permettere
    /// all'AuthService (in DataAccess) di generare il token senza dipendere dall'API.
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Crea un JWT firmato HMAC-SHA256 con i claim forniti.
        /// </summary>
        string CreateToken(IEnumerable<Claim> claims);

        /// <summary>
        /// Scadenza effettiva del token (UTC).
        /// </summary>
        DateTime GetExpiry();
    }
}
