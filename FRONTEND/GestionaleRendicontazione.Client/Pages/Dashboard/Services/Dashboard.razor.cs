using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Dashboard;

// Questa classe è suddivisa in più file (partial) per responsabilità:
//  - Dashboard.razor.cs          -> stato condiviso, ciclo di vita, caricamento worklog/lookup
//  - Dashboard.CreateWorklog.cs  -> modale "Nuovo Worklog"
//  - Dashboard.EditWorklog.cs    -> modale "Modifica Worklog"
//  - Dashboard.DeleteWorklog.cs  -> modale conferma eliminazione
//  - Dashboard.Report.cs         -> apertura/chiusura modale report
//
// Stato UI CRUD (IsLoading/IsSaving/ModalError/ErrorMessage) centralizzato in
// CrudPageService — qui restano solo lo stato applicativo (worklogs, periodo,
// lookup specifici del Dashboard, identity).
public partial class Dashboard : IDisposable
{
    [Inject] private CrudPageService Crud { get; set; } = default!;

    [CascadingParameter] private Task<AuthenticationState>? AuthStateTask { get; set; }

    private string _displayName = string.Empty;
    private List<WorkLogResponseDto> _worklogs = new();

    private DateOnly _periodFrom;
    private DateOnly _periodTo;
    private string _periodLabel => $"{_periodFrom:dd/MM/yyyy} – {_periodTo:dd/MM/yyyy}";

    private List<TypeResponseDto> _types = new();
    private List<EmployeeResponseDto> _employees = new();
    private bool _employeesLoading;

    private CancellationTokenSource? _loadCts;

    private IEnumerable<WorkLogResponseDto> FilteredWorklogs
    {
        get
        {
            if (string.IsNullOrWhiteSpace(FilterState.SearchQuery)) return _worklogs;

            var query = FilterState.SearchQuery.Trim();
            return _worklogs.Where(w =>
                Matches(w.ProjectName, query) ||
                Matches(w.Description, query) ||
                Matches(w.EmployeeName, query) ||
                Matches(w.TypeName, query) ||
                Matches(w.StatusName, query) ||
                Matches(w.Date.ToString("dd/MM/yyyy"), query) ||
                Matches(FormatHours(w.HoursCounter), query) ||
                Matches(w.HoursCounter.ToString("0.##", CultureInfo.InvariantCulture), query));
        }
    }

    private static bool Matches(string? source, string query) =>
        !string.IsNullOrEmpty(source) && source.Contains(query, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Restituisce l'ID (Guid) dell'utente attualmente autenticato leggendo il claim
    /// <see cref="ClaimTypes.NameIdentifier"/>. Ritorna <see cref="Guid.Empty"/> se il claim
    /// non è presente o non è un Guid valido. Usato per pre-popolare IdEmployee sui worklog
    /// creati da utenti non-admin (per loro il dipendente è sempre sé stessi).
    /// </summary>
    private Guid GetCurrentUserId()
    {
        if (AuthStateTask is null) return Guid.Empty;
        var authState = AuthStateTask.GetAwaiter().GetResult();
        var raw = authState.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }

    protected override void OnInitialized()
    {
        if (AuthStateTask is not null)
        {
            // Eventuale prima lettura sincrona; leggi nel seguito in modo async
            _ = ResolveDisplayNameAsync();
        }

        FilterState.OnFiltersChanged += HandleFiltersChanged;
        FilterState.OnLookupsChanged += HandleLookupsChanged;
        Crud.OnChanged += OnCrudStateChanged;

        ParseDatesFromService();
        _ = LoadInitialAsync();
    }

    private async Task ResolveDisplayNameAsync()
    {
        try
        {
            var authState = await AuthStateTask!;
            _displayName = authState.User.FindFirst("display_name")?.Value ?? authState.User.Identity?.Name ?? string.Empty;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore lettura display_name: {ex}");
        }
    }

    private void OnCrudStateChanged() => InvokeAsync(StateHasChanged);

    private async Task LoadInitialAsync()
    {
        await Task.WhenAll(LoadWorklogsAsync(), LoadDashboardSpecificLookupsAsync());
        if (FilterState.IsAdmin)
            await LoadEmployeesAsync();
    }

    private async void HandleLookupsChanged()
    {
        try
        {
            _types = await WorkLogApiClient.GetTypesAsync();
            if (FilterState.IsAdmin)
                await LoadEmployeesAsync();
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore reload tipologie/dipendenti dopo modifica admin: {ex}");
        }
    }

    private async Task LoadEmployeesAsync()
    {
        _employeesLoading = true;
        try
        {
            _employees = await WorkLogApiClient.GetEmployeesAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nel recupero dei dipendenti: {ex}");
            _employees = new();
        }
        finally
        {
            _employeesLoading = false;
        }
    }

    private void ParseDatesFromService()
    {
        if (DateOnly.TryParseExact(FilterState.FilterFromString, "yyyy-MM-dd", null, DateTimeStyles.None, out var parsedFrom))
            _periodFrom = parsedFrom;
        if (DateOnly.TryParseExact(FilterState.FilterToString, "yyyy-MM-dd", null, DateTimeStyles.None, out var parsedTo))
            _periodTo = parsedTo;
    }

    private async void HandleFiltersChanged()
    {
        try
        {
            ParseDatesFromService();
            await LoadWorklogsAsync();
            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException)
        {
            // Richiesta precedente cancellata da una nuova richiesta: ignoriamo
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore in HandleFiltersChanged: {ex}");
        }
    }

    private async Task LoadWorklogsAsync()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        var cts = new CancellationTokenSource();
        _loadCts = cts;
        var token = cts.Token;

        FilterState.Loading = true;
        Crud.SetErrorMessage(null);

        try
        {
            var result = await WorkLogApiClient.GetAsync(
                _periodFrom,
                _periodTo,
                FilterState.IsAdmin && FilterState.FilterEmployeeId != Guid.Empty ? FilterState.FilterEmployeeId : null,
                FilterState.IsAdmin && FilterState.FilterProjectId != Guid.Empty ? FilterState.FilterProjectId : null,
                FilterState.IsAdmin && !string.IsNullOrWhiteSpace(FilterState.FilterStatusName) ? FilterState.FilterStatusName : null,
                token);

            if (token.IsCancellationRequested) return;

            if (!result.IsSuccess)
            {
                Crud.SetErrorMessage(result.ToUserMessage("Impossibile recuperare i worklog dal server."));
                _worklogs = new();
                return;
            }

            _worklogs = (result.Data ?? new List<WorkLogResponseDto>())
                .OrderByDescending(w => w.Date)
                .ThenByDescending(w => w.CreateAt)
                .ToList();
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nel recupero dei worklog: {ex}");
            Crud.SetErrorMessage(ex.Message.Contains("HTTP", StringComparison.OrdinalIgnoreCase)
                ? ex.Message
                : "Impossibile recuperare i worklog dal server. Riprova più tardi.");
        }
        finally
        {
            if (!token.IsCancellationRequested)
                FilterState.Loading = false;
            Crud.NotifyStateChanged();
        }
    }

    private async Task LoadDashboardSpecificLookupsAsync()
    {
        try
        {
            _types = await WorkLogApiClient.GetTypesAsync();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nel recupero delle tipologie: {ex}");
        }
    }

    private static string FormatHours(float hours)
    {
        var formatted = hours.ToString("0.##", CultureInfo.InvariantCulture).Replace('.', ',');
        return $"{formatted} ore";
    }

    public void Dispose()
    {
        FilterState.OnFiltersChanged -= HandleFiltersChanged;
        FilterState.OnLookupsChanged -= HandleLookupsChanged;
        Crud.OnChanged -= OnCrudStateChanged;
        _loadCts?.Cancel();
    }
}