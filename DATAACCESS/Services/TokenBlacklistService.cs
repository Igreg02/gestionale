using System.Collections.Concurrent;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly ConcurrentDictionary<string, DateTime> _blacklist = new();

        public void BlacklistToken(string jti, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(jti)) return;

            PruneExpiredTokens();

            _blacklist.TryAdd(jti, expiresAt);
        }

        public bool IsBlacklisted(string jti)
        {
            if (string.IsNullOrWhiteSpace(jti)) return false;

            if (_blacklist.TryGetValue(jti, out var expiry))
            {
                if (DateTime.UtcNow < expiry)
                {
                    return true;
                }
                _blacklist.TryRemove(jti, out _);
            }
            return false;
        }

        private void PruneExpiredTokens()
        {
            var now = DateTime.UtcNow;
            foreach (var kvp in _blacklist)
            {
                if (kvp.Value <= now)
                {
                    _blacklist.TryRemove(kvp.Key, out _);
                }
            }
        }
    }
}
