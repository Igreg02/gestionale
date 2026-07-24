using AutoMapper;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Dtos;
using GestionaleRendicontazione.Domain.Entities;
using GestionaleRendicontazione.Domain.Interfaces;

namespace GestionaleRendicontazione.Dataaccess.Services
{
    public class LogService : ILogService
    {
        private const int DefaultPageSize = 50;
        private const int MaxPageSize = 500;

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
                // Normalizzazione parametri di paginazione
                if (page < 1) page = 1;
                if (pageSize <= 0) pageSize = DefaultPageSize;
                if (pageSize > MaxPageSize) pageSize = MaxPageSize;

                // Importante: inizializziamo la query con .Where(x => true) così
                // `var` deduce IQueryable<T> e le successive chiamate .Where()
                // (LINQ Queryable) si concatenano senza errori di cast.
                IQueryable<LogApplicativo> query = session.Query<LogApplicativo>().Where(l => true);

                // Filtro testo libero: contains su Messaggio, Path, UserId
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term = search.Trim();
                    query = query.Where(l =>
                        l.Messaggio.Contains(term) ||
                        l.Path.Contains(term) ||
                        l.UserId.Contains(term));
                }

                // Filtro per livello (es. "Information", "Warning", "Error"): match esatto
                if (!string.IsNullOrWhiteSpace(level))
                {
                    var lv = level.Trim();
                    query = query.Where(l => l.Livello == lv);
                }

                // Filtro per metodo HTTP: match esatto
                if (!string.IsNullOrWhiteSpace(method))
                {
                    var m = method.Trim();
                    query = query.Where(l => l.Metodo == m);
                }

                // Filtri per data
                if (dateFrom.HasValue)
                {
                    var from = dateFrom.Value;
                    query = query.Where(l => l.Data >= from);
                }

                if (dateTo.HasValue)
                {
                    var to = dateTo.Value;
                    query = query.Where(l => l.Data <= to);
                }

                // Conteggio totale PRIMA di applicare Skip/Take
                var total = query.Count();

                // Ordinamento: più recenti prima
                var items = query
                    .OrderByDescending(l => l.Data)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return new LogPage
                {
                    Items = _mapper.Map<List<LogDto.Response>>(items),
                    TotalCount = total,
                    Page = page,
                    PageSize = pageSize
                };
            }));
        }
    }
}
