using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GestionaleRendicontazione.Client.Services
{
    public class FilterStateService
    {
        private readonly WorkLogApiClient _apiClient;

        public FilterStateService(WorkLogApiClient apiClient)
        {
            _apiClient = apiClient;
            
            // Inizializzazione IMMEDIATA delle date al mese corrente per evitare lo stato 01/01/0001
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
        public List<EmployeeResponseDto> Employees { get; private set; } = new();
        public List<ProjectResponseDto> Projects { get; private set; } = new();
        public List<StatusResponseDto> Statuses { get; private set; } = new();

        public bool HasActiveFilters =>
            FilterEmployeeId != Guid.Empty || FilterProjectId != Guid.Empty || !string.IsNullOrWhiteSpace(FilterStatusName);

        public void NotifyFiltersChanged() => OnFiltersChanged?.Invoke();

        /// <summary>
        /// Riporta il servizio allo stato "pulito" quando cambia l'utente autenticato (login/logout)
        /// senza un refresh completo della pagina. Essendo registrato come Scoped, in Blazor WASM
        /// questo servizio vive per l'intera durata della tab del browser: senza questo reset, un
        /// logout/login rapido farebbe "ereditare" al nuovo utente IsAdmin, i filtri e le liste di
        /// lookup (es. Employees) della sessione precedente.
        /// </summary>
        public void ResetForNewSession()
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
        }

        public async Task LoadLookupsAsync()
        {
            try
            {
                var projTask = _apiClient.GetProjectsAsync();
                var statusTask = _apiClient.GetStatusesAsync();

                if (IsAdmin)
                {
                    var empTask = _apiClient.GetEmployeesAsync();
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
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore caricamento lookup nel FilterStateService: {ex}");
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

                await LoadLookupsAsync();
                if (token.IsCancellationRequested) return;

                OnLookupsChanged?.Invoke();
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore durante ReloadLookupsAsync: {ex}");
            }
        }
    }
}