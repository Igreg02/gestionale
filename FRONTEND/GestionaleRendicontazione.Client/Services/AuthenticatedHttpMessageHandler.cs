using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace GestionaleRendicontazione.Client.Services
{
    public sealed class AuthenticatedHttpMessageHandler : DelegatingHandler
    {
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

            return response;
        }
    }
}
