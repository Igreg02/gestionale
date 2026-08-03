using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Allega automaticamente il Bearer token a ogni richiesta verso il backend e intercetta le
    /// risposte 401 per forzare il logout locale (TDD §7, Fase F2 — "Gestione della scadenza").
    /// Un 401 su una richiesta a cui era stato allegato un token significa che il token non è più
    /// valido lato server: scaduto, oppure revocato tramite blacklist dopo un logout altrove
    /// (vedi TDD §2.3, TokenBlacklistService). In quel caso la sessione locale viene ripulita e
    /// l'utente viene rimandato al login, invece di restare "loggato" nella UI con un token morto.
    /// Un 401 senza token allegato (es. credenziali errate su /api/auth/login) non è una scadenza
    /// di sessione e resta gestito dal chiamante (AuthService.LoginAsync).
    ///
    /// Un 403 con l'header <see cref="PasswordChangeRequiredHeader"/> significa che il backend ha
    /// bloccato la richiesta perché l'utente deve cambiare password (PasswordChangeGate lato server,
    /// letto live dal DB — vedi API/Services/Auth/PasswordChangeGate.cs). A differenza del 401 qui
    /// l'utente NON va sloggato: il token resta valido, va solo rimandato alla pagina di cambio
    /// password. Serve come rete di sicurezza anche se il claim JWT locale è ancora "false" (es.
    /// l'Admin ha forzato il reset mentre l'utente aveva già un token in mano).
    /// </summary>
    public sealed class AuthenticatedHttpMessageHandler : DelegatingHandler
    {
        public const string PasswordChangeRequiredHeader = "X-Password-Change-Required";
        private readonly TokenStorageService _tokenStorageService;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private readonly NavigationManager _navigationManager;

        public AuthenticatedHttpMessageHandler(
            TokenStorageService tokenStorageService,
            CustomAuthenticationStateProvider authenticationStateProvider,
            NavigationManager navigationManager)
        {
            _tokenStorageService = tokenStorageService;
            _authenticationStateProvider = authenticationStateProvider;
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _tokenStorageService.GetTokenAsync();
            var hadToken = !string.IsNullOrWhiteSpace(token);
            if (hadToken)
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (hadToken && response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _authenticationStateProvider.MarkUserAsLoggedOutAsync();
                _navigationManager.NavigateTo("/login", forceLoad: false);
            }
            else if (hadToken && response.StatusCode == HttpStatusCode.Forbidden
                && response.Headers.Contains(PasswordChangeRequiredHeader))
            {
                _navigationManager.NavigateTo("/change-password", forceLoad: false);
            }

            return response;
        }
    }
}
