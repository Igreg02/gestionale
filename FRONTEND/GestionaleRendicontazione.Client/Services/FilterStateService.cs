using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

            var today = DateOnly.FromDateTime(DateTime.Today);
            var periodFrom = new DateOnly(today.Year, today.Month, 1);
            var periodTo = periodFrom.AddMonths(1).AddDays(-1);

            FilterFromString = periodFrom.ToString("yyyy-MM-dd");
            FilterToString = periodTo.ToString("yyyy-MM-dd");
        }

        public event Action? OnFiltersChanged;

        public event Action? OnLookupsChanged;

        public string SearchQuery { get; set; } = string.Empty;
        public string FilterFromString { get; set; }
        public string FilterToString { get; set; }
        public Guid FilterEmployeeId { get; set; } = Guid.Empty;
        public Guid FilterProjectId { get; set; } = Guid.Empty;
        public string FilterStatusName { get; set; } = string.Empty;

        public bool FilterPanelOpen { get; set; }
        public bool IsAdmin { get; set; }
        public bool Loading { get; set; }

        public List<EmployeeResponseDto> Employees { get; private set; } = new();
        public List<ProjectResponseDto> Projects { get; private set; } = new();
        public List<StatusResponseDto> Statuses { get; private set; } = new();

        public bool HasActiveFilters =>
            FilterEmployeeId != Guid.Empty || FilterProjectId != Guid.Empty || !string.IsNullOrWhiteSpace(FilterStatusName);

        public void NotifyFiltersChanged()
        {
            _ = SavePersistentFiltersAsync();
            OnFiltersChanged?.Invoke();
        }

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
            }
        }
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

            try { await _js.InvokeVoidAsync("gestionaleFilters.clear"); }
            catch {  }

            OnFiltersChanged?.Invoke();
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
