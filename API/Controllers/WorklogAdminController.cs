using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionaleRendicontazione.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = "Admin")]
    public class WorklogAdminController : ControllerBase
    {
        private readonly IWorkLogAdminService _workLogService;
        private readonly ILogger<WorklogAdminController> _logger;

        public WorklogAdminController(IWorkLogAdminService workLogService, ILogger<WorklogAdminController> logger)
        {
            _workLogService = workLogService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<WorkLogAdminDto.Response>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<WorkLogAdminDto.Response>>> GetAll(
            [FromQuery] Guid? employeeId,
            [FromQuery] Guid? projectId,
            [FromQuery] DateOnly? dateFrom,
            [FromQuery] DateOnly? dateTo,
            [FromQuery] string? statusName,
            CancellationToken ct)
        {
            var list = await _workLogService.GetAllAsync(employeeId, projectId, dateFrom, dateTo, statusName, ct);
            return Ok(list);
        }

        [HttpGet("{id:guid}", Name = "GetWorkLogById")]
        [ProducesResponseType(typeof(WorkLogAdminDto.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<WorkLogAdminDto.Response>> GetById(Guid id, CancellationToken ct)
        {
            var item = await _workLogService.GetByIdAsync(id, ct);
            if (item is null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        [ProducesResponseType(typeof(WorkLogAdminDto.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<WorkLogAdminDto.Response>> Create(
            [FromBody] WorkLogAdminDto.Create dto,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var created = await _workLogService.CreateAsync(dto, ct);
                return CreatedAtRoute("GetWorkLogById", new { id = created.Id }, created);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Creazione worklog fallita");
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
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<WorkLogAdminDto.Response>> Update(
            Guid id,
            [FromBody] WorkLogAdminDto.Update dto,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            try
            {
                var updated = await _workLogService.UpdateAsync(id, dto, ct);
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
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var ok = await _workLogService.DeleteAsync(id, ct);
            if (!ok) return NotFound();
            return NoContent();
        }
    }
}
