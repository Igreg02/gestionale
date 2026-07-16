using System.Text.Json;
using Microsoft.JSInterop;

namespace GestionaleRendicontazione.Client.Services
{
    public sealed class TokenStorageService
    {
        private const string StorageKey = "authSession";
        private readonly IJSRuntime _jsRuntime;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

        public TokenStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SaveSessionAsync(LoginResponseDto session)
        {
            var storedSession = new StoredSession(
                session.Token,
                session.ExpiresAt,
                session.UserName,
                session.DisplayName);

            var json = JsonSerializer.Serialize(storedSession, _jsonOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", StorageKey, json);
        }

        public async Task<StoredSession?> GetSessionAsync()
        {
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                var session = JsonSerializer.Deserialize<StoredSession>(json, _jsonOptions);
                if (session is null)
                {
                    await ClearSessionAsync();
                    return null;
                }

                if (session.ExpiresAt <= DateTime.UtcNow)
                {
                    await ClearSessionAsync();
                    return null;
                }

                return session;
            }
            catch
            {
                await ClearSessionAsync();
                return null;
            }
        }

        public async Task<string?> GetTokenAsync()
        {
            var session = await GetSessionAsync();
            return session?.Token;
        }

        public async Task ClearSessionAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", StorageKey);
        }
    }
}
