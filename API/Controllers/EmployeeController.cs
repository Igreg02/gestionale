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
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;
        private readonly ILogger<EmployeeController> _logger;

        public EmployeeController(IEmployeeService employeeService, ILogger<EmployeeController> logger)
        {
            _employeeService = employeeService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<EmployeeDto.Response>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<List<EmployeeDto.Response>>> GetAll(CancellationToken ct)
        {
            var list = await _employeeService.GetAllAsync(ct);
            return Ok(list);
        }

        [HttpGet("{id:guid}", Name = "GetEmployeeById")]
        [ProducesResponseType(typeof(EmployeeDto.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<EmployeeDto.Response>> GetById(Guid id, CancellationToken ct)
        {
            var item = await _employeeService.GetByIdAsync(id, ct);
            if (item is null) return NotFound();
            return Ok(item);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(EmployeeDto.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<EmployeeDto.Response>> Update(
            Guid id,
            [FromBody] EmployeeDto.Update dto,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var updated = await _employeeService.UpdateAsync(id, dto, ct);
            if (updated is null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var ok = await _employeeService.DeleteAsync(id, ct);
            if (!ok) return NotFound();
            _logger.LogInformation("Dipendente {Id} licenziato", id);
            return NoContent();
        }
    }
}
