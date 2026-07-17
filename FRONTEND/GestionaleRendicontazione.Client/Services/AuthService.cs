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

        /// <summary>
        /// Crea un nuovo Employee (solo Admin) tramite POST /api/auth/register.
        /// Il backend restituisce 400 per errori di validazione (es. password troppo corta) e
        /// 422 se lo userName è già in uso.
        /// </summary>
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
                // ignora: se il body non è un ProblemDetails valido usiamo il messaggio di default del chiamante
            }

            return null;
        }
    }
}
