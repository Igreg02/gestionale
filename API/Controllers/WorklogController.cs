using System.Security.Claims;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionaleRendicontazione.Api.Controllers
{
    /// <summary>
    /// Controller unico per la gestione dei worklog, con route condivise tra Admin e utente normale.
    /// - Ruolo "Admin": accesso completo (tutti i dipendenti, filtri avanzati, IdEmployee libero in Create).
    /// - Utente normale: accede solo ai propri worklog. L'IdEmployee non è mai preso dal client:
    ///   viene sempre forzato a quello del dipendente autenticato (claim NameIdentifier), anche se il body
    ///   ne contiene uno diverso, per evitare che un utente possa impersonare un altro dipendente.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class WorklogController : ControllerBase
    {
        private readonly IWorkLogAdminService _adminService;
        private readonly IWorkLogUserService _userService;

        public WorklogController(
            IWorkLogAdminService adminService,
            IWorkLogUserService userService)
        {
            _adminService = adminService;
            _userService = userService;
        }

        private bool IsAdmin => User.IsInRole(RoleNames.Admin);

        [HttpGet]
        [ProducesResponseType(typeof(List<WorkLogDto.Admin.Response>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll(
            [FromQuery] Guid? employeeId,
            [FromQuery] Guid? projectId,
            [FromQuery] DateOnly? dateFrom,
            [FromQuery] DateOnly? dateTo,
            [FromQuery] string? statusName,
            CancellationToken ct)
        {
            if (IsAdmin)
            {
                var list = await _adminService.GetAllAsync(employeeId, projectId, dateFrom, dateTo, statusName, ct);
                return Ok(list);
            }

            // Un utente normale non può filtrare per employeeId/projectId/statusName altrui:
            // vede solo i propri worklog, indipendentemente da cosa passa in query.
            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId is null) return Unauthorized();

            var ownList = await _userService.GetAllAsync(currentEmployeeId.Value, dateFrom, dateTo, ct);
            return Ok(ownList);
        }

        [HttpGet("{id:guid}", Name = "GetWorkLogById")]
        [ProducesResponseType(typeof(WorkLogDto.Admin.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        {
            if (IsAdmin)
            {
                var item = await _adminService.GetByIdAsync(id, ct);
                if (item is null) return NotFound();
                return Ok(item);
            }

            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId is null) return Unauthorized();

            var ownItem = await _userService.GetByIdAsync(id, currentEmployeeId.Value, ct);
            if (ownItem is null) return NotFound();
            return Ok(ownItem);
        }

        [HttpPost]
        [ProducesResponseType(typeof(WorkLogDto.Admin.Response), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Create(
            [FromBody] WorkLogDto.Admin.Create dto,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

                if (IsAdmin)
                {
                    var created = await _adminService.CreateAsync(dto, ct);
                    return CreatedAtRoute("GetWorkLogById", new { id = created.Id }, created);
                }

                var currentEmployeeId = GetCurrentEmployeeId();
                if (currentEmployeeId is null) return Unauthorized();

                // IdEmployee dal body viene ignorato: si usa sempre quello del token.
                var userDto = new WorkLogDto.User.Create
                {
                    Description = dto.Description,
                    HoursCounter = dto.HoursCounter,
                    Date = dto.Date,
                    IdProject = dto.IdProject,
                    IdType = dto.IdType,
                    IdStatus = dto.IdStatus
                };

                var ownCreated = await _userService.CreateAsync(userDto, currentEmployeeId.Value, ct);
                return CreatedAtRoute("GetWorkLogById", new { id = ownCreated.Id }, ownCreated);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(WorkLogDto.Admin.Response), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] WorkLogDto.Admin.Update dto,
            CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);


                if (IsAdmin)
                {
                    var updated = await _adminService.UpdateAsync(id, dto, ct);
                    if (updated is null) return NotFound();
                    return Ok(updated);
                }

                var currentEmployeeId = GetCurrentEmployeeId();
                if (currentEmployeeId is null) return Unauthorized();

                var userDto = new WorkLogDto.User.Update
                {
                    Description = dto.Description,
                    HoursCounter = dto.HoursCounter,
                    Date = dto.Date,
                    IdProject = dto.IdProject,
                    IdType = dto.IdType,
                    IdStatus = dto.IdStatus
                };

                var ownUpdated = await _userService.UpdateAsync(id, userDto, currentEmployeeId.Value, ct);
                if (ownUpdated is null) return NotFound();
                return Ok(ownUpdated);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            if (IsAdmin)
            {
                var ok = await _adminService.DeleteAsync(id, ct);
                if (!ok) return NotFound();
                return NoContent();
            }

            var currentEmployeeId = GetCurrentEmployeeId();
            if (currentEmployeeId is null) return Unauthorized();

            var ownOk = await _userService.DeleteAsync(id, currentEmployeeId.Value, ct);
            if (!ownOk) return NotFound();
            return NoContent();
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