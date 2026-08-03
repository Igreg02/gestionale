using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GestionaleRendicontazione.Client.Models;
using Microsoft.JSInterop;

namespace GestionaleRendicontazione.Client.Services
{
    public class FilterStateService
    {
        private readonly WorkLogApiClient _apiClient;
        private readonly IJSRuntime _js;

        public FilterStateService(WorkLogApiClient apiClient, IJSRuntime js)
        {
            _apiClient = apiClient;
            _js = js;

            // Inizializzazione IMMEDIATA delle date al mese corrente per evitare lo stato 01/01/0001.
            // Se in localStorage c'è uno stato precedente, verrà sovrascritto da LoadFromStorageAsync()
            // (chiamato dal MainLayout al boot); qui impostiamo i default per il primo render.
            var today = DateOnly.FromDateTime(DateTime.Today);
            var periodFrom = new DateOnly(today.Year, today.Month, 1);
            var periodTo = periodFrom.AddMonths(1).AddDays(-1);

            FilterFromString = periodFrom.ToString("yyyy-MM-dd");
            FilterToString = periodTo.ToString("yyyy-MM-dd");
        }

        public event Action? OnFiltersChanged;

        /// <summary>
        /// Scatta quando le liste di lookup (Projects, Statuses, Employees) sono state ricaricate
        /// dal server — ad esempio dopo un Create/Update/Delete da una pagina admin. Le pagine
        /// interessate (es. Dashboard) possono sottoscriversi per aggiornare le proprie select.
        /// </summary>
        public event Action? OnLookupsChanged;

        /// <summary>
        /// Scatta quando un caricamento/ricaricamento delle lookup fallisce, così le pagine
        /// possono mostrare un feedback visibile invece di lasciare solo il log in console.
        /// </summary>
        public event Action? LookupsLoadFailed;

        // Stato dei Filtri applicati
        public string SearchQuery { get; set; } = string.Empty;
        public string FilterFromString { get; set; }
        public string FilterToString { get; set; }
        public Guid FilterEmployeeId { get; set; } = Guid.Empty;
        public Guid FilterProjectId { get; set; } = Guid.Empty;
        public string FilterStatusName { get; set; } = string.Empty;

        // Stati di UI e permessi
        public bool FilterPanelOpen { get; set; }
        public bool IsAdmin { get; set; }
        public bool Loading { get; set; }

        // Liste di lookup centralizzate
        public List<EmployeeResponse> Employees { get; private set; } = new();
        public List<ProjectResponse> Projects { get; private set; } = new();
        public List<StatusResponse> Statuses { get; private set; } = new();

        public bool HasActiveFilters =>
            FilterEmployeeId != Guid.Empty || FilterProjectId != Guid.Empty || !string.IsNullOrWhiteSpace(FilterStatusName);

        public void NotifyFiltersChanged()
        {
            // Persisti i filtri "di sessione lunga" (date + selezioni dropdown)
            // ogni volta che cambiano. Le operazioni sono asincrone fire-and-forget:
            // localStorage è veloce (<1ms) e non blocca il render di Blazor.
            _ = SavePersistentFiltersAsync();
            OnFiltersChanged?.Invoke();
        }

        /// <summary>
        /// Ripristina i filtri persistenti salvati in localStorage (date + selezioni dropdown).
        /// Chiamato dal MainLayout al boot. Se uno dei valori admin-only era salvato ma l'utente
        /// corrente non è più admin, viene scartato per evitare filtri "invisibili" all'utente.
        /// </summary>
        public async Task LoadFromStorageAsync()
        {
            try
            {
                var data = await _js.InvokeAsync<PersistentFilters?>("gestionaleFilters.load");
                if (data is null) return;

                if (!string.IsNullOrWhiteSpace(data.FilterFrom))
                    FilterFromString = data.FilterFrom;
                if (!string.IsNullOrWhiteSpace(data.FilterTo))
                    FilterToString = data.FilterTo;

                // I filtri admin-only sono significativi solo se l'utente è admin.
                // Senza questo check, un non-admin che condivide il browser vedrebbe
                // lavorlog filtrati per un altro dipendente senza poter rimuovere
                // il filtro (il select dipendente è nascosto).
                if (IsAdmin)
                {
                    if (Guid.TryParse(data.EmployeeId, out var empId) && empId != Guid.Empty)
                        FilterEmployeeId = empId;
                    if (Guid.TryParse(data.ProjectId, out var projId) && projId != Guid.Empty)
                        FilterProjectId = projId;
                    if (!string.IsNullOrWhiteSpace(data.StatusName))
                        FilterStatusName = data.StatusName;
                }
            }
            catch (Exception ex)
            {
                // JS non ancora disponibile o localStorage corrotto: ignora,
                // i default del costruttore restano in vigore.
                Console.Error.WriteLine($"Errore caricamento filtri da localStorage: {ex}");
            }
        }

        private async Task SavePersistentFiltersAsync()
        {
            try
            {
                var data = new PersistentFilters
                {
                    FilterFrom = FilterFromString,
                    FilterTo = FilterToString,
                    EmployeeId = FilterEmployeeId == Guid.Empty ? string.Empty : FilterEmployeeId.ToString(),
                    ProjectId = FilterProjectId == Guid.Empty ? string.Empty : FilterProjectId.ToString(),
                    StatusName = FilterStatusName ?? string.Empty
                };
                await _js.InvokeVoidAsync("gestionaleFilters.save", data);
            }
            catch
            {
                // Ambiente non browser (test/SSR) o localStorage pieno: ignora.
                // Lo stato in memoria resta valido per la sessione corrente.
            }
        }

        /// <summary>
        /// Riporta il servizio allo stato "pulito" quando cambia l'utente autenticato (login/logout)
        /// senza un refresh completo della pagina. Essendo registrato come Scoped, in Blazor WASM
        /// questo servizio vive per l'intera durata della tab del browser: senza questo reset, un
        /// logout/login rapido farebbe "ereditare" al nuovo utente IsAdmin, i filtri e le liste di
        /// lookup (es. Employees) della sessione precedente.
        /// </summary>
        public async Task ResetForNewSession()
        {
            IsAdmin = false;
            Employees = new();
            Projects = new();
            Statuses = new();

            SearchQuery = string.Empty;
            FilterEmployeeId = Guid.Empty;
            FilterProjectId = Guid.Empty;
            FilterStatusName = string.Empty;
            FilterPanelOpen = false;

            // Le date (FilterFromString/FilterToString) NON vengono resettate —
            // anche dopo un logout/login ha senso mantenere il "periodo di
            // osservazione" preferito dall'utente. Il filtro dipendente/progetto/stato
            // viene azzerato per evitare leak cross-account.

            // Pulisce anche lo storage per evitare che i filtri admin-only del
            // precedente utente "resistano" sul nuovo account non-admin.
            try { await _js.InvokeVoidAsync("gestionaleFilters.clear"); }
            catch { /* ignora — ambiente non browser */ }

            OnFiltersChanged?.Invoke();
        }

        public async Task LoadLookupsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var projTask = _apiClient.GetProjectsAsync(cancellationToken);
                var statusTask = _apiClient.GetStatusesAsync(cancellationToken);

                if (IsAdmin)
                {
                    var empTask = _apiClient.GetEmployeesAsync(cancellationToken);
                    await Task.WhenAll(projTask, statusTask, empTask);
                    Employees = await empTask;
                }
                else
                {
                    await Task.WhenAll(projTask, statusTask);
                }

                Projects = await projTask;
                Statuses = await statusTask;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore caricamento lookup nel FilterStateService: {ex}");
                LookupsLoadFailed?.Invoke();
            }
        }
        private static readonly TimeSpan ReloadDebounce = TimeSpan.FromMilliseconds(500);
        private CancellationTokenSource? _reloadCts;

        /// <summary>
        /// Ricarica le liste di lookup dal server e notifica gli ascoltatori di <see cref="OnLookupsChanged"/>.
        /// Da chiamare dalle pagine admin dopo un Create/Update/Delete andato a buon fine.
        /// È debounced internamente per evitare N reload quando l'admin fa molte modifiche di fila.
        /// </summary>
        public async Task ReloadLookupsAsync()
        {
            _reloadCts?.Cancel();
            _reloadCts?.Dispose();
            var cts = new CancellationTokenSource();
            _reloadCts = cts;
            var token = cts.Token;

            try
            {
                await Task.Delay(ReloadDebounce, token);
                if (token.IsCancellationRequested) return;

                await LoadLookupsAsync(token);
                if (token.IsCancellationRequested) return;

                OnLookupsChanged?.Invoke();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore durante ReloadLookupsAsync: {ex}");
                LookupsLoadFailed?.Invoke();
            }
        }

        /// <summary>
        /// DTO serializzato in localStorage per ricordare i filtri Dashboard tra
        /// refresh/navigazione. Campi nullable/empty-string-safe.
        /// </summary>
        private sealed class PersistentFilters
        {
            public string FilterFrom { get; set; } = string.Empty;
            public string FilterTo { get; set; } = string.Empty;
            public string EmployeeId { get; set; } = string.Empty;
            public string ProjectId { get; set; } = string.Empty;
            public string StatusName { get; set; } = string.Empty;
        }
    }
}
