using System.Security.Claims;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
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

        public async Task<AuthDto.LoginResponseDto?> LoginAsync(AuthDto.LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request is null
                || string.IsNullOrWhiteSpace(request.UserName)
                || string.IsNullOrWhiteSpace(request.Password))
            {
                return null;
            }


            var (verifyResult, newHash, userOid, userName, firstName, lastName, roles) =
                _dbContextService.ExecuteReadOnly(session =>
            {
                var user = session.FindObject<Employee>(
                    new BinaryOperator(nameof(Employee.UserName), request.UserName));

                if (user is null || string.IsNullOrEmpty(user.PasswordHash))
                {
                    return (PasswordVerificationResult.Failed, string.Empty, Guid.Empty,
                            string.Empty, string.Empty, string.Empty, Array.Empty<string>());
                }

                var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
                if (result == PasswordVerificationResult.Failed)
                {
                    return (PasswordVerificationResult.Failed, string.Empty, Guid.Empty,
                            string.Empty, string.Empty, string.Empty, Array.Empty<string>());
                }

                string rehash = string.Empty;
                if (result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    rehash = _passwordHasher.HashPassword(user, request.Password);
                }

                var snapshot = user.Roles
                    .Where(r => !string.IsNullOrWhiteSpace(r.Name))
                    .Select(r => r.Name!)
                    .ToArray();

                return (result, rehash, user.Oid, user.UserName ?? string.Empty,
                        user.FirstName ?? string.Empty, user.LastName ?? string.Empty, snapshot);
            });

            if (verifyResult == PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded
                && !string.IsNullOrEmpty(newHash))
            {
                await _dbContextService.ReadWrite(async uow =>
                {
                    var reload = await uow.GetObjectByKeyAsync<Employee>(userOid, cancellationToken);
                    if (reload is not null)
                    {
                        reload.PasswordHash = newHash;
                    }
                });
            }

            var claims = BuildClaims(userOid, userName, firstName, lastName, roles);
            var token = _jwtTokenService.CreateToken(claims);
            var expiresAt = _jwtTokenService.GetExpiry();

            var displayName = BuildDisplayName(firstName, lastName, userName);

            return new AuthDto.LoginResponseDto(token, expiresAt, userName, displayName);
        }

        public async Task<AuthDto.RegisterResponseDto?> RegisterAsync(AuthDto.RegisterRequestDto request, CancellationToken cancellationToken = default)
        {
            if (request is null
                || string.IsNullOrWhiteSpace(request.UserName)
                || string.IsNullOrWhiteSpace(request.Password))
            {
                return null;
            }

            var userExists = _dbContextService.ExecuteReadOnly(session =>
            {
                return session.FindObject<Employee>(new BinaryOperator(nameof(Employee.UserName), request.UserName)) is not null;
            });

            if (userExists)
            {
                return null;
            }

            AuthDto.RegisterResponseDto? responseDto = null;

            await _dbContextService.ReadWrite(async uow =>
            {
                var newEmployee = new Employee(uow)
                {
                    UserName = request.UserName,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    IsActive = true
                };

                newEmployee.PasswordHash = _passwordHasher.HashPassword(newEmployee, request.Password);

                // Assegnazione del ruolo di default "User" sul database
                var userRole = uow.Query<PermissionPolicyRole>().FirstOrDefault(r => r.Name == "User");

                if (userRole != null)
                {
                    newEmployee.Roles.Add(userRole);
                }
                else
                {
                    var defaultRole = new PermissionPolicyRole(uow)
                    {
                        Name = "User",
                    };
                    newEmployee.Roles.Add(defaultRole);
                }


                responseDto = new AuthDto.RegisterResponseDto(
                    newEmployee.Oid,
                    newEmployee.UserName,
                    newEmployee.FirstName,
                    newEmployee.LastName,
                    newEmployee.IsActive
                );
            });

            return responseDto;
        }

        private static string BuildDisplayName(string firstName, string lastName, string userName)
        {
            var first = firstName?.Trim() ?? string.Empty;
            var last = lastName?.Trim() ?? string.Empty;
            var full = $"{first} {last}".Trim();
            return string.IsNullOrEmpty(full) ? userName : full;
        }

        private static IEnumerable<Claim> BuildClaims(Guid oid, string userName, string firstName, string lastName, IReadOnlyList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim("sub", oid.ToString()),
                new Claim("jti", Guid.NewGuid().ToString()),
                // ClaimTypes.Name contiene lo UserName (vedi commento su NameClaimType in Program.cs).
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.NameIdentifier, oid.ToString())
            };

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                claims.Add(new Claim("given_name", firstName));
            }
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                claims.Add(new Claim("family_name", lastName));
            }

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            return claims;
        }
    }
}