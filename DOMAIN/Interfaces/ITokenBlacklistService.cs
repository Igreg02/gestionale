using System;
using System.Threading.Tasks;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface ITokenBlacklistService
    {
        Task BlacklistTokenAsync(string jti, DateTime expiresAt);
        Task<bool> IsBlacklistedAsync(string jti);
    }
}
