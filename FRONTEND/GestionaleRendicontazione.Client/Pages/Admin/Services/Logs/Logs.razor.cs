using GestionaleRendicontazione.Client.Models;
using GestionaleRendicontazione.Client.Services;
using Microsoft.AspNetCore.Components;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Pagina di sola consultazione: nessuna modale di creazione/modifica/eliminazione,
// a differenza delle altre pagine Admin — i log sono alimentati esclusivamente dal
// sink Serilog lato server (vedi LogController).
// I filtri (search + date/level/method) vivono nella topbar, gestiti da
// LogFilterStateService. Questa pagina reagisce a OnFiltersChanged e ricarica.
public partial class Logs
{
    [Inject] private LogApiClient LogApi { get; set; } = default!;
    [Inject] private LogFilterStateService LogFilterState { get; set; } = default!;

    private const int PageSize = 50;

    private bool _loading = true;
    private string? _errorMessage;
    private List<LogResponse> _items = new();

    private int _page = 1;
    private int _totalCount;

    private bool _viewOpen;
    private LogResponse? _viewTarget;

    private CancellationTokenSource? _loadCts;

    private int _totalPages => _totalCount == 0 ? 1 : (int)Math.Ceiling(_totalCount / (double)PageSize);
    private int _firstRowIndex => _totalCount == 0 ? 0 : ((_page - 1) * PageSize) + 1;
    private int _lastRowIndex => Math.Min(_page * PageSize, _totalCount);

    protected override async Task OnInitializedAsync()
    {
        LogFilterState.OnFiltersChanged += OnFilterStateChanged;
        await LoadAsync();
    }

    private void OnFilterStateChanged()
    {
        // Trigger scatenato dalla topbar (ricerca o Filtra/Reset). Debounce già
        // applicato lato topbar: qui resettiamo la pagina e ricarichiamo.
        _ = InvokeAsync(async () =>
        {
            _page = 1;
            await LoadAsync();
        });
    }

    private async Task LoadAsync()
    {
        _loadCts?.Cancel();
        var cts = new CancellationTokenSource();
        _loadCts = cts;

        _loading = true;
        _errorMessage = null;
        StateHasChanged();

        try
        {
            var result = await LogApi.GetAllAsync(
                search: LogFilterState.SearchQuery,
                level: string.IsNullOrWhiteSpace(LogFilterState.FilterLevel) ? null : LogFilterState.FilterLevel,
                method: string.IsNullOrWhiteSpace(LogFilterState.FilterMethod) ? null : LogFilterState.FilterMethod,
                dateFrom: LogFilterState.TryGetDateFrom(),
                dateTo: LogFilterState.TryGetDateTo(),
                page: _page,
                pageSize: PageSize,
                cancellationToken: cts.Token);

            if (cts.Token.IsCancellationRequested) return;

            if (result.IsSuccess && result.Data is not null)
            {
                _items = result.Data.Items;
                _totalCount = result.Data.TotalCount;
                _page = result.Data.Page;
            }
            else
            {
                _errorMessage = result.ToUserMessage("Impossibile recuperare i log dal server. Riprova più tardi.");
            }
        }
        catch (OperationCanceledException)
        {
            // Superata da una ricerca/filtro più recente: nessun errore da mostrare.
        }
        catch (Exception ex)
        {
            if (cts.Token.IsCancellationRequested) return;
            Console.Error.WriteLine($"Errore nel recupero dei log: {ex}");
            _errorMessage = "Impossibile recuperare i log dal server. Riprova più tardi.";
        }
        finally
        {
            if (!cts.Token.IsCancellationRequested)
            {
                _loading = false;
                StateHasChanged();
            }
        }
    }

    private void OpenView(LogResponse log)
    {
        _viewTarget = log;
        _viewOpen = true;
    }

    private void CloseView()
    {
        _viewOpen = false;
        _viewTarget = null;
    }

    private async Task GoToPageAsync(int page)
    {
        if (page < 1 || page > _totalPages || page == _page) return;
        _page = page;
        await LoadAsync();
    }

    public void Dispose()
    {
        LogFilterState.OnFiltersChanged -= OnFilterStateChanged;
        _loadCts?.Cancel();
    }
}
