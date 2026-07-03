using System.Security.Claims;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface IJwtTokenService
    {

        string CreateToken(IEnumerable<Claim> claims);

        DateTime GetExpiry();
    }
}
