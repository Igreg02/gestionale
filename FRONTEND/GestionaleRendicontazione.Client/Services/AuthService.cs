using System.Net.Http.Json;
using System.Text.Json;

namespace GestionaleRendicontazione.Client.Services
{
    public sealed class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public AuthService(
            HttpClient httpClient,
            CustomAuthenticationStateProvider authenticationStateProvider)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
        }

        public async Task<bool> LoginAsync(string userName, string password)
        {
            var request = new LoginRequestDto
            {
                UserName = userName,
                Password = password
            };
            using var response = await _httpClient.PostAsJsonAsync("api/auth/login", request, _jsonOptions);

            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);
            if (loginResponse is null)
            {
                return false;
            }

            await _authenticationStateProvider.MarkUserAsAuthenticatedAsync(loginResponse);
            return true;
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _httpClient.PostAsync("api/auth/logout", null);
            }
            catch
            {
                // Ignoriamo eventuali errori di logout perché il client deve comunque pulire lo stato locale.
            }

            await _authenticationStateProvider.MarkUserAsLoggedOutAsync();
        }
    }
}
