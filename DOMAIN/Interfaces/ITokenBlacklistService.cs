namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface ITokenBlacklistService
    {
        void BlacklistToken(string jti, DateTime expiresAt);
        bool IsBlacklisted(string jti);
    }
}
