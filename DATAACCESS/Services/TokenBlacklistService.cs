using System;
using System.Linq;
using System.Threading.Tasks;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IDbContextService _dbContextService;

        public TokenBlacklistService(IDbContextService dbContextService)
        {
            _dbContextService = dbContextService;
        }

        public async Task BlacklistTokenAsync(string jti, DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(jti)) return;

            // Inseriamo il token in blacklist nel DB
            await _dbContextService.ReadWriteAsync(async uow =>
            {
                var existing = await uow.GetObjectByKeyAsync<BlacklistedToken>(jti);
                if (existing is null)
                {
                    _ = new BlacklistedToken(uow)
                    {
                        Jti = jti,
                        ExpiresAt = expiresAt
                    };
                }
            });

            // Avviamo anche una potatura asincrona dei token scaduti per pulire la tabella
            _ = PruneExpiredTokensAsync();
        }

        public Task<bool> IsBlacklistedAsync(string jti)
        {
            if (string.IsNullOrWhiteSpace(jti)) return Task.FromResult(false);

            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var token = session.GetObjectByKey<BlacklistedToken>(jti);
                if (token is not null)
                {
                    if (DateTime.UtcNow < token.ExpiresAt)
                    {
                        return true;
                    }
                }
                return false;
            }));
        }

        private async Task PruneExpiredTokensAsync()
        {
            try
            {
                await _dbContextService.ReadWriteAsync(async uow =>
                {
                    var now = DateTime.UtcNow;
                    var expiredTokens = uow.Query<BlacklistedToken>()
                        .Where(t => t.ExpiresAt <= now)
                        .ToList();

                    if (expiredTokens.Any())
                    {
                        uow.Delete(expiredTokens);
                    }
                    await Task.CompletedTask;
                });
            }
            catch
            {
                // Ignoriamo silenti errori di manutenzione in background per non bloccare la chiamata principale
            }
        }
    }
}
