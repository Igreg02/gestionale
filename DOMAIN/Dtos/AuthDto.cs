using System.ComponentModel.DataAnnotations;

namespace GestionaleRendicontazione.Domain.Dtos
{
    public class AuthDto
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

        public record RegisterRequestDto
        {
            [Required(AllowEmptyStrings = false, ErrorMessage = "userName obbligatorio")]
            [StringLength(100, MinimumLength = 1)]
            public string UserName { get; init; } = string.Empty;

            [Required(AllowEmptyStrings = false, ErrorMessage = "password obbligatoria")]
            [StringLength(256, MinimumLength = 6, ErrorMessage = "La password deve essere di almeno 6 caratteri")]
            public string Password { get; init; } = string.Empty;

            [Required(AllowEmptyStrings = false, ErrorMessage = "firstName obbligatorio")]
            [StringLength(100)]
            public string FirstName { get; init; } = string.Empty;

            [Required(AllowEmptyStrings = false, ErrorMessage = "lastName obbligatorio")]
            [StringLength(100)]
            public string LastName { get; init; } = string.Empty;
        }

        public record RegisterResponseDto(
            Guid Oid,
            string UserName,
            string FirstName,
            string LastName,
            bool IsActive);
    }
}
