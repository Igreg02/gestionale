using System.Security.Claims;
using GestionaleRendicontazione.Client.Models;
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

        /// <summary>
        /// Costruisce l'identità a partire dai claim reali contenuti nel JWT (inclusi i ruoli), invece
        /// che da soli UserName/DisplayName: è il presupposto perché &lt;AuthorizeView Roles="Admin"&gt;
        /// e le route protette per ruolo (Fase F3) funzionino correttamente. Il DisplayName restituito
        /// dal login (comodo per la UI ma non presente nel token) viene aggiunto come claim separato.
        /// </summary>
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
                // Token presente ma non decodificabile (corrotto/manomesso): trattiamo l'utente come
                // anonimo piuttosto che fallire l'intera pagina. Al prossimo giro TokenStorageService
                // lo ripulirà comunque se anche la scadenza risulta superata.
                return new ClaimsIdentity();
            }
        }
    }
}
