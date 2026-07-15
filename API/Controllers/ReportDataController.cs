using System.Security.Claims;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


//TODO: E' una BOZZA ancora da testare

//TODO: ReportDataController valida from > to ma non un range massimo (rischio query enormi su periodi anni).


namespace GestionaleRendicontazione.Api.Controllers
{
    [ApiController]
    [Route("api/report-data")]
    [Produces("application/json")]
    public class ReportDataController : ControllerBase
    {
        private readonly IReportService _reportService;
        private readonly ILogger<ReportDataController> _logger;

        public ReportDataController(IReportService reportService, ILogger<ReportDataController> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        /// <summary>Admin: report aggregato per progetto/periodo.</summary>
        [HttpGet("project/{projectId:guid}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(ProjectReportDto.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ProjectReportDto.Response>> GetProjectReport(
            Guid projectId,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            CancellationToken ct)
        {
            if (from > to)
            {
                return Problem(
                    title: "Intervallo date non valido",
                    detail: "'from' deve essere minore o uguale a 'to'.",
                    statusCode: StatusCodes.Status400BadRequest);
            }
            if (from.AddYears(1) < to)
            {
                return Problem(
                    title: "Intervallo date troppo ampio",
                    detail: "L'intervallo di date massimo consentito è di 1 anno.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var report = await _reportService.GetProjectReportAsync(projectId, from, to, ct);
            if (report is null) return NotFound();
            return Ok(report);
        }

        /// <summary>Admin: report aggregato per dipendente/periodo (id esplicito).</summary>
        [HttpGet("employee/{employeeId:guid}")]
        [Authorize(Roles = RoleNames.Admin)]
        [ProducesResponseType(typeof(EmployeeReportDto.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<EmployeeReportDto.Response>> GetEmployeeReport(
            Guid employeeId,
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            CancellationToken ct)
        {
            if (from > to)
            {
                return Problem(
                    title: "Intervallo date non valido",
                    detail: "'from' deve essere minore o uguale a 'to'.",
                    statusCode: StatusCodes.Status400BadRequest);
            }
            if (from.AddYears(1) < to)
            {
                return Problem(
                    title: "Intervallo date troppo ampio",
                    detail: "L'intervallo di date massimo consentito è di 1 anno.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var report = await _reportService.GetEmployeeReportAsync(employeeId, from, to, ct);
            if (report is null) return NotFound();
            return Ok(report);
        }

        /// <summary>User: report aggregato del dipendente autenticato/periodo.</summary>
        [HttpGet("employee")]
        [Authorize]
        [ProducesResponseType(typeof(EmployeeReportDto.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<EmployeeReportDto.Response>> GetMyReport(
            [FromQuery] DateOnly from,
            [FromQuery] DateOnly to,
            CancellationToken ct)
        {
            if (from > to)
            {
                return Problem(
                    title: "Intervallo date non valido",
                    detail: "'from' deve essere minore o uguale a 'to'.",
                    statusCode: StatusCodes.Status400BadRequest);
            }
            if (from.AddYears(1) < to)
            {
                return Problem(
                    title: "Intervallo date troppo ampio",
                    detail: "L'intervallo di date massimo consentito è di 1 anno.",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var employeeId = GetCurrentEmployeeId();
            if (employeeId is null) return Unauthorized();

            var report = await _reportService.GetEmployeeReportAsync(employeeId.Value, from, to, ct);
            if (report is null) return NotFound();
            return Ok(report);
        }

        /// <summary>
        /// Estrae l'Id del dipendente autenticato dal claim "NameIdentifier" (popolato da JwtTokenService).
        /// Ritorna null se il claim manca o non è un Guid valido.
        /// </summary>
        private Guid? GetCurrentEmployeeId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(raw, out var parsed) ? parsed : (Guid?)null;
        }
    }
}
