using GestionaleRendicontazione.Client.Services;
using Microsoft.AspNetCore.Components;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Pagina di sola consultazione: nessuna modale di creazione/modifica/eliminazione,
// a differenza delle altre pagine Admin — i log sono alimentati esclusivamente dal
// sink Serilog lato server (vedi LogController).
public partial class Logs
{
    private const int PageSize = 50;
    private const int SearchDebounceMs = 350;

    private bool _loading = true;
    private string? _errorMessage;
    private List<LogResponse> _items = new();

    private string _search = string.Empty;
    private string _level = string.Empty;
    private string _method = string.Empty;
    private string _dateFrom = string.Empty;
    private string _dateTo = string.Empty;

    private int _page = 1;
    private int _totalCount;

    private bool _filterPanelOpen;
    private bool _viewOpen;
    private LogResponse? _viewTarget;

    private System.Timers.Timer? _searchDebounceTimer;
    private CancellationTokenSource? _loadCts;

    private bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(_level) || !string.IsNullOrWhiteSpace(_method)
        || !string.IsNullOrWhiteSpace(_dateFrom) || !string.IsNullOrWhiteSpace(_dateTo);

    private int _totalPages => _totalCount == 0 ? 1 : (int)Math.Ceiling(_totalCount / (double)PageSize);
    private int _firstRowIndex => _totalCount == 0 ? 0 : ((_page - 1) * PageSize) + 1;
    private int _lastRowIndex => Math.Min(_page * PageSize, _totalCount);

    protected override async Task OnInitializedAsync() => await LoadAsync();

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
            var result = await LogApiClient.GetAllAsync(
                search: _search,
                level: _level,
                method: _method,
                dateFrom: ParseDate(_dateFrom),
                dateTo: ParseDate(_dateTo),
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

    private static DateOnly? ParseDate(string value)
        => DateOnly.TryParse(value, out var d) ? d : null;

    private void OnSearchInput(ChangeEventArgs e)
    {
        _search = e.Value?.ToString() ?? string.Empty;

        _searchDebounceTimer?.Stop();
        _searchDebounceTimer?.Dispose();
        _searchDebounceTimer = new System.Timers.Timer(SearchDebounceMs) { AutoReset = false };
        _searchDebounceTimer.Elapsed += (_, _) =>
        {
            _searchDebounceTimer?.Dispose();
            _searchDebounceTimer = null;
            InvokeAsync(ApplyFiltersAsync);
        };
        _searchDebounceTimer.Start();
    }

    private void OnDateFromChanged(ChangeEventArgs e) => _dateFrom = e.Value?.ToString() ?? string.Empty;
    private void OnDateToChanged(ChangeEventArgs e) => _dateTo = e.Value?.ToString() ?? string.Empty;

    private void ClearSearch()
    {
        _searchDebounceTimer?.Stop();
        _searchDebounceTimer?.Dispose();
        _searchDebounceTimer = null;
        _search = string.Empty;
        _ = InvokeAsync(ApplyFiltersAsync);
    }

    private void ToggleFilterPanel() => _filterPanelOpen = !_filterPanelOpen;

    private async Task ApplyFiltersAsync()
    {
        _filterPanelOpen = false;
        _page = 1;
        await LoadAsync();
    }

    private async Task ResetFiltersAsync()
    {
        _search = string.Empty;
        _level = string.Empty;
        _method = string.Empty;
        _dateFrom = string.Empty;
        _dateTo = string.Empty;
        _filterPanelOpen = false;
        _page = 1;
        await LoadAsync();
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
        _searchDebounceTimer?.Stop();
        _searchDebounceTimer?.Dispose();
        _loadCts?.Cancel();
    }
}