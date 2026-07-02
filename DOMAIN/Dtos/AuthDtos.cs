using System.ComponentModel.DataAnnotations;

namespace GestionaleRendicontazione.Domain.Dtos
{

    public record LoginRequestDto
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "userName obbligatorio")]
        [StringLength(100, MinimumLength = 1)]
        public string UserName { get; init; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "password obbligatoria")]
        [StringLength(256, MinimumLength = 1)]
        public string Password { get; init; } = string.Empty;
    }


    public record LoginResponseDto(
        string Token,
        DateTime ExpiresAt,
        string UserName,
        string DisplayName);


    public record LogoutResponseDto(string Message);
}
