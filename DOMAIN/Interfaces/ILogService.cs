using GestionaleRendicontazione.Domain.Dtos;

namespace GestionaleRendicontazione.Domain.Interfaces
{
    public interface ILogService
    {
        Task<LogPage> GetAllAsync(
            string? search = null,
            string? level = null,
            string? method = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 50,
            CancellationToken ct = default);
    }

    public class LogPage
    {
        public List<LogDto.Response> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
