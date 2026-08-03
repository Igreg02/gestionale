namespace GestionaleRendicontazione.Client.Models
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

    public sealed class RegisterRequestDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }

    public sealed record RegisterResponseDto(
        Guid Id,
        string UserName,
        string FirstName,
        string LastName,
        bool IsActive);

    public sealed record StoredSession(
        string Token,
        DateTime ExpiresAt,
        string UserName,
        string DisplayName);

    public sealed class ChangePasswordRequestDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
