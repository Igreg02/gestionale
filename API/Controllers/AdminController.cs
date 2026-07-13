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
    public class AdminController : ControllerBase
    {
        private readonly IWorkLogService _workLogService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IWorkLogService workLogService, ILogger<AdminController> logger)
        {
            _workLogService = workLogService;
            _logger = logger;
        }

        [HttpGet("worklogs")]
        [ProducesResponseType(typeof(List<WorkLogAdminDto.Response>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<WorkLogAdminDto.Response>>> GetAll(CancellationToken ct)
        {
            var list = await _workLogService.GetAllAsync(ct);
            return Ok(list);
        }

        [HttpGet("worklogs/{id:guid}", Name = "GetWorkLogById")]
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

        [HttpPost("worklogs")]
        [ProducesResponseType(typeof(WorkLogAdminDto.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<WorkLogAdminDto.Response>> Create(
            [FromBody] WorkLogAdminDto.Create dto,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var created = await _workLogService.CreateAsync(dto, ct);
            return CreatedAtRoute("GetWorkLogById", new { id = created.Id }, created);
        }

        [HttpPut("worklogs/{id:guid}")]
        [ProducesResponseType(typeof(WorkLogAdminDto.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<WorkLogAdminDto.Response>> Update(
            Guid id,
            [FromBody] WorkLogAdminDto.Update dto,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var updated = await _workLogService.UpdateAsync(id, dto, ct);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("worklogs/{id:guid}")]
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
