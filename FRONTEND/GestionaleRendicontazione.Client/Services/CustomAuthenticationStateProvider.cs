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

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, session.UserName),
                new Claim(ClaimTypes.NameIdentifier, session.UserName),
                new Claim("display_name", session.DisplayName)
            }, "Bearer");

            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        public async Task MarkUserAsAuthenticatedAsync(LoginResponseDto session)
        {
            await _tokenStorageService.SaveSessionAsync(session);
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, session.UserName),
                new Claim(ClaimTypes.NameIdentifier, session.UserName),
                new Claim("display_name", session.DisplayName)
            }, "Bearer");

            var authState = new AuthenticationState(new ClaimsPrincipal(identity));
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        public async Task MarkUserAsLoggedOutAsync()
        {
            await _tokenStorageService.ClearSessionAsync();
            var anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            NotifyAuthenticationStateChanged(Task.FromResult(anonymous));
        }
    }
}
