using GestionaleRendicontazione.Domain.Entities;

namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// Mappatura condivisa tra <see cref="WorkLog"/> e i DTO di risposta.
    /// Evita la duplicazione della stessa proiezione in WorkLogAdminService e WorkLogUserService.
    /// </summary>
    public static class WorkLogMapper
    {
        /// <summary>
        /// Proiezione completa, usata lato Admin (include dipendente e timestamp).
        /// </summary>
        public static WorkLogDto.Admin.Response ToAdminResponse(WorkLog w)
        {
            return new WorkLogDto.Admin.Response
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

        /// <summary>
        /// Proiezione ridotta, usata lato User (nessun dato sul dipendente o sui timestamp).
        /// </summary>
        public static WorkLogDto.User.Response ToUserResponse(WorkLog w)
        {
            return new WorkLogDto.User.Response
            {
                Id = w.Id,
                Description = w.Description,
                HoursCounter = w.HoursCounter,
                Date = w.Date,
                IdProject = w.Project?.Id ?? Guid.Empty,
                ProjectName = w.Project?.Name ?? string.Empty,
                IdType = w.Type?.Id ?? Guid.Empty,
                TypeName = w.Type?.Name ?? string.Empty,
                IdStatus = w.Status?.Id ?? Guid.Empty,
                StatusName = w.Status?.Name ?? string.Empty
            };
        }
    }
}