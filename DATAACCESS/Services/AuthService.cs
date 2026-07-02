using System.Security.Claims;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using GestionaleRendicontazione.Dataaccess.Datacontext.DbContextService;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GestionaleRendicontazione.Dataaccess.Services
{

    public class AuthService : IAuthService
    {
        private readonly IDbContextService _dbContextService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly PasswordHasher<Employee> _passwordHasher;

        public AuthService(
            IDbContextService dbContextService,
            IJwtTokenService jwtTokenService,
            PasswordHasher<Employee> passwordHasher)
        {
            _dbContextService = dbContextService;
            _jwtTokenService = jwtTokenService;
            _passwordHasher = passwordHasher;
        }

        public Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request is null
                || string.IsNullOrWhiteSpace(request.UserName)
                || string.IsNullOrWhiteSpace(request.Password))
            {
                return Task.FromResult<LoginResponseDto?>(null);
            }

            // Sola lettura: la password va verificata, l'utente recuperato, nessuna scrittura qui.
            var response = _dbContextService.ExecuteReadOnly(session =>
            {
                var user = session.FindObject<Employee>(
                    new BinaryOperator(nameof(Employee.UserName), request.UserName));

                if (user is null)
                {
                    return null;
                }

                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    // Utente presente ma senza hash: rifiuto l'autenticazione.
                    return null;
                }

                var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
                if (verifyResult == PasswordVerificationResult.Failed)
                {
                    return null;
                }

                // Aggiorna il contatore/hash solo se l'algoritmo di hash è cambiato (SuccessRehash).
                if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    var newHash = _passwordHasher.HashPassword(user, request.Password);
                    // Riscrittura: eseguita in una sessione separata di UnitOfWork.
                    _dbContextService.ReadWrite(async uow =>
                    {
                        var reload = await uow.GetObjectByKeyAsync<Employee>(user.Oid);
                        if (reload is not null)
                        {
                            reload.PasswordHash = newHash;
                            await uow.CommitChangesAsync();
                        }
                    }).GetAwaiter().GetResult();
                }

                var claims = BuildClaims(user);
                var token = _jwtTokenService.CreateToken(claims);
                var expiresAt = _jwtTokenService.GetExpiry();

                var displayName = BuildDisplayName(user);

                return new LoginResponseDto(token, expiresAt, user.UserName, displayName);
            });

            return Task.FromResult(response);
        }

        private static string BuildDisplayName(Employee user)
        {
            var first = user.FirstName?.Trim() ?? string.Empty;
            var last = user.LastName?.Trim() ?? string.Empty;
            var full = $"{first} {last}".Trim();
            return string.IsNullOrEmpty(full) ? (user.UserName ?? string.Empty) : full;
        }

        private static IEnumerable<Claim> BuildClaims(Employee user)
        {
            var claims = new List<Claim>
            {
                // Claim standard "sub" = identificativo univoco dell'utente (Oid XPO).
                new Claim("sub", user.Oid.ToString()),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Oid.ToString())
            };

            // Aggiungo i ruoli XAF, se presenti.
            foreach (var role in user.Roles)
            {
                if (!string.IsNullOrWhiteSpace(role.Name))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }
            }

            // Ruolo di default: ogni utente autenticato è almeno "User".
            if (!claims.Any(c => c.Type == ClaimTypes.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, "User"));
            }

            return claims;
        }
    }
}
