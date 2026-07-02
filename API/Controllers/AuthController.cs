using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionaleRendicontazione.Api.Controllers
{
    /// <summary>
    /// Controller di autenticazione. Espone gli endpoint di login e logout in coerenza
    /// con il TDD (JWT Bearer Token, header Authorization). Conforme al §3 del TDD:
    /// risposte JSON, 401 per credenziali errate, ProblemDetails (RFC 7807) per gli errori.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }




        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(
            [FromBody] LoginRequestDto request,
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
        public IActionResult Logout()
        {
            var userName = User?.Identity?.Name ?? "(sconosciuto)";
            _logger.LogInformation("Logout richiesto per {UserName}", userName);
            return NoContent();
        }
    }
}
