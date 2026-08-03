using GestionaleRendicontazione.Api.Helpers;
using GestionaleRendicontazione.Api.Helpers.Audit;
using GestionaleRendicontazione.Api.Services.Auth;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GestionaleRendicontazione.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenBlacklistService _blacklistService;
        private readonly IAuditLogger _audit;

        public AuthController(
            IAuthService authService,
            ITokenBlacklistService blacklistService,
            IAuditLogger audit)
        {
            _authService = authService;
            _blacklistService = blacklistService;
            _audit = audit;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        [ProducesResponseType(typeof(AuthDto.LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login(
            [FromBody] AuthDto.LoginRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.LoginAsync(request, cancellationToken);
            if (result is null)
            {
                _audit.AuthEvent("Login", request.UserName, success: false);
                return Problem(
                    title: "Credenziali non valide",
                    detail: "userName o password errati.",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            _audit.AuthEvent("Login", request.UserName, success: true);
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        [AllowPasswordChangeRequired]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var userName = User.GetUserName() ?? "(sconosciuto)";

            var jti = User.FindFirst("jti")?.Value;
            if (string.IsNullOrEmpty(jti))
            {
                // Nessun jti sul token: non possiamo revocarlo lato server.
                // Tracciato come fallimento di audit invece di sparire silenziosamente.
                _audit.AuthEvent("Logout", userName, success: false);
                return NoContent();
            }

            var expClaim = User.FindFirst("exp")?.Value;
            var expiresAt = DateTime.UtcNow.AddHours(1); // fallback
            if (long.TryParse(expClaim, out var expUnix))
            {
                expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            }
            await _blacklistService.BlacklistTokenAsync(jti, expiresAt);

            _audit.AuthEvent("Logout", userName, success: true);
            return NoContent();
        }

        [HttpPost("register")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(AuthDto.RegisterResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Register([FromBody] AuthDto.RegisterRequestDto request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var result = await _authService.RegisterAsync(request, cancellationToken);

            if (result is null)
            {
                _audit.AuthEvent("Register", request.UserName, success: false);
                return Problem(
                    title: "Registrazione fallita",
                    detail: "Impossibile creare l'utente. Lo userName potrebbe essere già in uso o la password non soddisfa i requisiti.",
                    statusCode: StatusCodes.Status422UnprocessableEntity);
            }

            _audit.AuthEvent("Register", request.UserName, success: true);
            return Ok(result);
        }

        [HttpPost("change-password")]
        [Authorize]
        [AllowPasswordChangeRequired]
        [ProducesResponseType(typeof(AuthDto.LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword(
            [FromBody] AuthDto.ChangePasswordRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(ModelState);
            }

            var employeeId = User.GetEmployeeId();
            if (employeeId is null)
            {
                return Problem(
                    title: "Token non valido",
                    detail: "Impossibile identificare l'utente autenticato.",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var result = await _authService.ChangePasswordAsync(
                employeeId.Value, request.CurrentPassword, request.NewPassword, cancellationToken);

            if (result is null)
            {
                _audit.AuthEvent("ChangePassword", User.GetUserName() ?? "(sconosciuto)", success: false);
                return Problem(
                    title: "Cambio password fallito",
                    detail: "La password attuale non è corretta.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            _audit.AuthEvent("ChangePassword", User.GetUserName() ?? "(sconosciuto)", success: true);
            return Ok(result);
        }
    }
}
