using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionaleRendicontazione.Api.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenBlacklistService _blacklistService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthService authService,
            ITokenBlacklistService blacklistService,
            ILogger<AuthController> logger)
        {
            _authService = authService;
            _blacklistService = blacklistService;
            _logger = logger;
        }



        // TODO: AGGIUNGERE MESSAGGIO D'ERRORE PER LOGIN FALLITO

        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthDto.LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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
                _logger.LogWarning("Login fallito per userName={UserName}", request.UserName);
                return Problem(
                    title: "Credenziali non valide",
                    detail: "userName o password errati.",
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            _logger.LogInformation("Login riuscito per userName={UserName}", request.UserName);
            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                           ?? User.FindFirst("unique_name")?.Value
                           ?? User.Identity?.Name
                           ?? "(sconosciuto)";

            var jti = User.FindFirst("jti")?.Value;
            var expClaim = User.FindFirst("exp")?.Value;
            if (!string.IsNullOrEmpty(jti))
            {
                var expiresAt = DateTime.UtcNow.AddHours(1); // fallback
                if (long.TryParse(expClaim, out var expUnix))
                {
                    expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
                }
                await _blacklistService.BlacklistTokenAsync(jti, expiresAt);
                _logger.LogInformation("Token JTI={Jti} inserito in blacklist per {UserName}", jti, userName);
            }

            _logger.LogInformation("Logout completato per {UserName}", userName);
            return NoContent();
        }

        [HttpPost("register")]
        [Authorize]
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
                _logger.LogWarning("Registrazione fallita per userName={UserName}", request.UserName);
                return Problem(
                    title: "Registrazione fallita",
                    detail: "Impossibile creare l'utente. Lo userName potrebbe essere già in uso o la password non soddisfa i requisiti.",
                    statusCode: StatusCodes.Status422UnprocessableEntity);
            }

            _logger.LogInformation("Registrazione completata con successo per userName={UserName}", request.UserName);
            return Ok(result);
        }
    }
}
