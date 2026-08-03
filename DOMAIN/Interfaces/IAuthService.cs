using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface IAuthService
    {
        Task<AuthDto.LoginResponseDto?> LoginAsync(AuthDto.LoginRequestDto request, CancellationToken cancellationToken = default);

        Task<AuthDto.RegisterResponseDto?> RegisterAsync(AuthDto.RegisterRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cambia la password dell'employee autenticato dopo aver verificato la password attuale.
        /// Su successo azzera <see cref="Domain.Entities.Employee.MustChangePassword"/> e ritorna un
        /// token fresco (coi claim aggiornati) così il client non deve rifare login.
        /// Ritorna null se employeeId non esiste o la password attuale non è corretta.
        /// </summary>
        Task<AuthDto.LoginResponseDto?> ChangePasswordAsync(Guid employeeId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);
    }
}
