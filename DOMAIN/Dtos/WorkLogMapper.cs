using GestionaleRendicontazione.Domain.Entities;

namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Mappatura condivisa tra <see cref="WorkLog"/> e <see cref="WorkLogAdminDto.Response"/>.
    /// Evita la duplicazione della stessa proiezione in WorkLogAdminService e WorkLogUserService.
    /// </summary>
    public static class WorkLogMapper
    {
        public static WorkLogAdminDto.Response ToResponse(WorkLog w)
        {
            return new WorkLogAdminDto.Response
            {
                Id = w.Id,
                Description = w.Description,
                HoursCounter = w.HoursCounter,
                Date = w.Date,
                CreateAt = w.CreateAt,
                UpdateAt = w.UpdateAt,
                IdProject = w.Project?.Id ?? Guid.Empty,
                ProjectName = w.Project?.Name ?? string.Empty,
                IdEmploy = w.Employee?.Oid ?? Guid.Empty,
                EmployeeName = w.Employee?.UserName ?? string.Empty,
                IdType = w.Type?.Id ?? Guid.Empty,
                TypeName = w.Type?.Name ?? string.Empty,
                IdStatus = w.Status?.Id ?? Guid.Empty,
                StatusName = w.Status?.Name ?? string.Empty
            };
        }
    }
}
