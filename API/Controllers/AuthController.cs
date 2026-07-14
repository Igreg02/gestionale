using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Interfaces;
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
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
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
        public IActionResult Logout()
        {
            // Recupera lo username esplicito cercando prima ClaimTypes.Name, poi unique_name
            var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value
                           ?? User.FindFirst("unique_name")?.Value
                           ?? User.Identity?.Name
                           ?? "(sconosciuto)";
        
            _logger.LogInformation("Logout richiesto per {UserName}", userName);
            return NoContent();
        }

            [HttpPost("register")]
            [Authorize]
            [Authorize(Roles = "Admin")]
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
