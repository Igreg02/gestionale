using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    /// <summary>
    /// Contratto per il servizio di autenticazione. Implementato in DataAccess/Services.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Verifica le credenziali e, se valide, restituisce il DTO di risposta con il token JWT.
        /// Restituisce <c>null</c> in caso di credenziali errate.
        /// </summary>
        Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Registra un nuovo dipendente nel sistema previa verifica di univocità dello UserName.
        /// Restituisce il DTO con i dettagli dell'utente creato, oppure <c>null</c> se lo UserName è già esistente.
        /// </summary>
        Task<RegisterResponseDto?> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    }
}
