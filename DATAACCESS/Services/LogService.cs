using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;
using GestionaleRendicontazione.Dataaccess.Helpers;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class LogService : ILogService
    {
        private readonly IDbContextService _dbContextService;
        private readonly IMapper _mapper;

        public LogService(IDbContextService dbContextService, IMapper mapper)
        {
            _dbContextService = dbContextService;
            _mapper = mapper;
        }

        public Task<LogPage> GetAllAsync(
            string? search = null,
            string? level = null,
            string? method = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int page = 1,
            int pageSize = 50,
            CancellationToken ct = default)
        {
            return Task.FromResult(_dbContextService.ExecuteReadOnly(session =>
            {
                var (p, ps) = LogQueryBuilder.NormalizePaging(page, pageSize);
                var query = session.Query<LogApplicativo>()
                    .ApplyFilters(search, level, method, dateFrom, dateTo);

                var total = query.Count();
                var items = query
                    .OrderByDescending(l => l.Data)
                    .Skip((p - 1) * ps)
                    .Take(ps)
                    .ToList();

                return new LogPage
                {
                    Items = _mapper.Map<List<LogDto.Response>>(items),
                    TotalCount = total,
                    Page = p,
                    PageSize = ps
                };
            }));
        }
    }
}
