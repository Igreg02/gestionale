using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace GestionaleRendicontazione.Client.Services
{
    public sealed class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly TokenStorageService _tokenStorageService;

        public CustomAuthenticationStateProvider(TokenStorageService tokenStorageService)
        {
            _tokenStorageService = tokenStorageService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var session = await _tokenStorageService.GetSessionAsync();
            if (session is null)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            return new AuthenticationState(new ClaimsPrincipal(BuildIdentity(session)));
        }

        public async Task MarkUserAsAuthenticatedAsync(LoginResponseDto session)
        {
            await _tokenStorageService.SaveSessionAsync(session);

            var stored = new StoredSession(session.Token, session.ExpiresAt, session.UserName, session.DisplayName);
            var authState = new AuthenticationState(new ClaimsPrincipal(BuildIdentity(stored)));
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        public async Task MarkUserAsLoggedOutAsync()
        {
            await _tokenStorageService.ClearSessionAsync();
            var anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            NotifyAuthenticationStateChanged(Task.FromResult(anonymous));
        }

        private static ClaimsIdentity BuildIdentity(StoredSession session)
        {
            try
            {
                var claims = JwtParser.ParseClaimsFromJwt(session.Token).ToList();
                claims.Add(new Claim("display_name", session.DisplayName));

                return new ClaimsIdentity(
                    claims,
                    authenticationType: "Bearer",
                    nameType: ClaimTypes.Name,
                    roleType: ClaimTypes.Role);
            }
            catch
            {
                return new ClaimsIdentity();
            }
        }
    }
}
