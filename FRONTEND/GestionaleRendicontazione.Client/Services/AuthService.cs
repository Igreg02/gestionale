using System.Net.Http.Json;
using System.Text.Json;
using GestionaleRendicontazione.Client.Constants;

namespace GestionaleRendicontazione.Client.Services
{
    public sealed class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly CustomAuthenticationStateProvider _authenticationStateProvider;
        private readonly FilterStateService _filterState;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public AuthService(
            HttpClient httpClient,
            CustomAuthenticationStateProvider authenticationStateProvider,
            FilterStateService filterState)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _filterState = filterState;
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

            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            _filterState.IsAdmin = authState.User.IsInRole(RoleNames.Admin);
            await _filterState.LoadLookupsAsync();

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
            }

            await _authenticationStateProvider.MarkUserAsLoggedOutAsync();

            await _filterState.ResetForNewSession();
        }

        public async Task<ApiResult<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request)
        {
            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync("api/auth/register", request, _jsonOptions);
            }
            catch (Exception)
            {
                return ApiResult<RegisterResponseDto>.WithError("Errore di rete. Riprova più tardi.");
            }

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<RegisterResponseDto>(_jsonOptions);
                return ApiResult<RegisterResponseDto>.Ok(data!);
            }

            if ((int)response.StatusCode == 422)
            {
                var detail = await TryReadProblemDetailAsync(response);
                return ApiResult<RegisterResponseDto>.WithError(
                    detail ?? "Impossibile creare l'utente. Lo username potrebbe essere già in uso.");
            }

            return await response.ToApiResultAsync<RegisterResponseDto>();
        }

        private static async Task<string?> TryReadProblemDetailAsync(HttpResponseMessage response)
        {
            try
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("detail", out var detailEl))
                {
                    return detailEl.GetString();
                }
            }
            catch
            {
            }

            return null;
        }
    }
}
