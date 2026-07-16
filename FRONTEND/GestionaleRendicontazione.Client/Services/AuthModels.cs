using System;

namespace GestionaleRendicontazione.Client.Services
{
    public sealed class LoginRequestDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public sealed record LoginResponseDto(
        string Token,
        DateTime ExpiresAt,
        string UserName,
        string DisplayName);

    public sealed record StoredSession(
        string Token,
        DateTime ExpiresAt,
        string UserName,
        string DisplayName);
}
