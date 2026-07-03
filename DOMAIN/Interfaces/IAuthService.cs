using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<AuthDto.LoginResponseDto?> LoginAsync(AuthDto.LoginRequestDto request, CancellationToken cancellationToken = default);

        Task<AuthDto.RegisterResponseDto?> RegisterAsync(AuthDto.RegisterRequestDto request, CancellationToken cancellationToken = default);
    }
}
