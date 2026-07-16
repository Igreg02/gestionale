using System.Net.Http.Headers;

namespace GestionaleRendicontazione.Client.Services
{
    public sealed class AuthenticatedHttpMessageHandler : DelegatingHandler
    {
        private readonly TokenStorageService _tokenStorageService;

        public AuthenticatedHttpMessageHandler(TokenStorageService tokenStorageService)
        {
            _tokenStorageService = tokenStorageService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _tokenStorageService.GetTokenAsync();
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
