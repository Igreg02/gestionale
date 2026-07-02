using System.Security.Claims;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Persistent.BaseImpl.PermissionPolicy;
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
                    return null;
                }

                var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
                if (verifyResult == PasswordVerificationResult.Failed)
                {
                    return null;
                }

                if (verifyResult == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    var newHash = _passwordHasher.HashPassword(user, request.Password);
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

        public async Task<RegisterResponseDto?> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
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

            RegisterResponseDto? responseDto = null;

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
                        IsAdministrative = false
                    };
                    newEmployee.Roles.Add(defaultRole);
                }

                await uow.CommitChangesAsync(cancellationToken);

                responseDto = new RegisterResponseDto(
                    newEmployee.Oid,
                    newEmployee.UserName,
                    newEmployee.FirstName,
                    newEmployee.LastName,
                    newEmployee.IsActive
                );
            });

            return responseDto;
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
                new Claim("sub", user.Oid.ToString()),
                new Claim("jti", Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Oid.ToString())
            };

            foreach (var role in user.Roles)
            {
                if (!string.IsNullOrWhiteSpace(role.Name))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }
            }

            return claims;
        }
    }
}