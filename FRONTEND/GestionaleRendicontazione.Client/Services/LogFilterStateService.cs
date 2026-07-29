using System;
using System.Threading.Tasks;

namespace GestionaleRendicontazione.Client.Services
{
    public class LogFilterStateService
    {
        public LogFilterStateService()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var periodFrom = new DateOnly(today.Year, today.Month, 1);
            var periodTo = periodFrom.AddMonths(1).AddDays(-1);

            FilterFromString = periodFrom.ToString("yyyy-MM-dd");
            FilterToString = periodTo.ToString("yyyy-MM-dd");
        }

        public event Action? OnFiltersChanged;

        public string SearchQuery { get; set; } = string.Empty;
        public string FilterFromString { get; set; }
        public string FilterToString { get; set; }
        public string FilterLevel { get; set; } = string.Empty;
        public string FilterMethod { get; set; } = string.Empty;

        public bool FilterPanelOpen { get; set; }

        public bool HasActiveFilters =>
            !string.IsNullOrWhiteSpace(FilterLevel) ||
            !string.IsNullOrWhiteSpace(FilterMethod);

        public DateOnly? TryGetDateFrom()
            => DateOnly.TryParse(FilterFromString, out var d) ? d : null;

        public DateOnly? TryGetDateTo()
            => DateOnly.TryParse(FilterToString, out var d) ? d : null;

        public void NotifyFiltersChanged() => OnFiltersChanged?.Invoke();

        public async Task ReloadAsync()
        {
            await Task.Yield();
            OnFiltersChanged?.Invoke();
        }
    }
}
