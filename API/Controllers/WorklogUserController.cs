using System.Security.Claims;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionaleRendicontazione.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class WorklogUserController : ControllerBase
    {
        private readonly IWorkLogUserService _workLogService;
        private readonly ILogger<WorklogUserController> _logger;

        public WorklogUserController(IWorkLogUserService workLogService, ILogger<WorklogUserController> logger)
        {
            _workLogService = workLogService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<WorkLogAdminDto.Response>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<List<WorkLogAdminDto.Response>>> GetAll(
            [FromQuery] DateOnly? dateFrom,
            [FromQuery] DateOnly? dateTo,
            CancellationToken ct)
        {
            var employeeOid = GetCurrentEmployeeOid();
            if (employeeOid is null) return Unauthorized();
            var list = await _workLogService.GetAllAsync(employeeOid.Value, dateFrom, dateTo, ct);
            return Ok(list);
        }

        [HttpGet("{id:guid}", Name = "GetUserWorkLogById")]
        [ProducesResponseType(typeof(WorkLogAdminDto.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<WorkLogAdminDto.Response>> GetById(Guid id, CancellationToken ct)
        {
            var employeeOid = GetCurrentEmployeeOid();
            if (employeeOid is null) return Unauthorized();
            var item = await _workLogService.GetByIdAsync(id, employeeOid.Value, ct);
            if (item is null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        [ProducesResponseType(typeof(WorkLogAdminDto.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<WorkLogAdminDto.Response>> Create(
            [FromBody] WorkLogAdminDto.Create dto,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var employeeOid = GetCurrentEmployeeOid();
            if (employeeOid is null) return Unauthorized();

            try
            {
                var created = await _workLogService.CreateAsync(dto, employeeOid.Value, ct);
                return CreatedAtRoute("GetUserWorkLogById", new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Creazione worklog personale fallita");
                return Problem(
                    title: "Creazione worklog fallita",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status422UnprocessableEntity);
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(WorkLogAdminDto.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<WorkLogAdminDto.Response>> Update(
            Guid id,
            [FromBody] WorkLogAdminDto.Update dto,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var employeeOid = GetCurrentEmployeeOid();
            if (employeeOid is null) return Unauthorized();

            try
            {
                var updated = await _workLogService.UpdateAsync(id, dto, employeeOid.Value, ct);
                if (updated is null) return NotFound();
                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Aggiornamento worklog {Id} fallito", id);
                return Problem(
                    title: "Aggiornamento worklog fallito",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status422UnprocessableEntity);
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var employeeOid = GetCurrentEmployeeOid();
            if (employeeOid is null) return Unauthorized();
            var ok = await _workLogService.DeleteAsync(id, employeeOid.Value, ct);
            if (!ok) return NotFound();
            return NoContent();
        }

        /// <summary>
        /// Estrae l'Oid del dipendente autenticato dal claim "NameIdentifier" (popolato da JwtTokenService).
        /// Ritorna null se il claim manca o non è un Guid valido.
        /// </summary>
        private Guid? GetCurrentEmployeeOid()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var parsed) ? parsed : (Guid?)null;
        }
    }
}
