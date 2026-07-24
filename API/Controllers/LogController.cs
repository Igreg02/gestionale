using GestionaleRendicontazione.Domain.Constants;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionaleRendicontazione.Api.Controllers
{
    /// <summary>
    /// Controller per la consultazione dei log applicativi.
    /// I log sono scritti esclusivamente dal sink Serilog; questo controller
    /// espone solo la GET per la pagina admin. Riservato al ruolo Admin.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize(Roles = RoleNames.Admin)]
    public class LogController : ControllerBase
    {
        private readonly ILogService _logService;

        public LogController(ILogService logService)
        {
            _logService = logService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(LogPage), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<LogPage>> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? level,
            [FromQuery] string? method,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            CancellationToken ct = default)
        {
            var result = await _logService.GetAllAsync(
                search: search,
                level: level,
                method: method,
                dateFrom: dateFrom,
                dateTo: dateTo,
                page: page,
                pageSize: pageSize,
                ct: ct);

            return Ok(result);
        }
    }
}
