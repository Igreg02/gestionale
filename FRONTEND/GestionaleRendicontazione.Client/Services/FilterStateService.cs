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
    }
}