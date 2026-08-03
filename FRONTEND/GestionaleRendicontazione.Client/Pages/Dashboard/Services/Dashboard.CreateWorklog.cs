using System.Globalization;
using Microsoft.AspNetCore.Components;
using GestionaleRendicontazione.Client.Models;
using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Dashboard;

// Logica della modale "Nuovo Worklog".
// Lo stato UI (IsSaving/ModalError) vive in CrudPageService; qui restano solo
// lo stato locale del form (modello, data string, flag modal-open) e la chiamata
// API specifica di WorkLogApiClient.CreateAsync.
public partial class Dashboard
{
    private bool _createModalOpen;
    private WorkLogUpdateRequestDto? _createModel;
    private string _createDateString = string.Empty;

    private async Task OpenCreateModal()
    {
        Crud.ResetModalError();
        if (FilterState.IsAdmin && _employees.Count == 0)
        {
            await LoadEmployeesAsync();
        }
        var today = DateOnly.FromDateTime(DateTime.Today);
        // Per l'utente non-admin il dipendente è sé stesso: lo pre-popoliamo con il proprio ID
        // ricavato dai claim (NameIdentifier = id utente). L'admin invece sceglie liberamente.
        var defaultEmployeeId = FilterState.IsAdmin
            ? Guid.Empty
            : await GetCurrentUserIdAsync();
        _createModel = new WorkLogUpdateRequestDto
        {
            Date = today,
            HoursCounter = 8,
            IdEmployee = defaultEmployeeId,
            IdProject = Guid.Empty,
            IdType = Guid.Empty,
            IdStatus = Guid.Empty,
        };
        _createDateString = today.ToString("yyyy-MM-dd");
        _createModalOpen = true;
    }

    private void CloseCreateModal()
    {
        _createModalOpen = false;
        _createModel = null;
        Crud.ResetModalError();
    }

    private void OnCreateDateInput(ChangeEventArgs e)
    {
        _createDateString = e.Value?.ToString() ?? string.Empty;
        if (_createModel is not null && DateOnly.TryParseExact(_createDateString, "yyyy-MM-dd", null, DateTimeStyles.None, out var parsed))
        {
            _createModel.Date = parsed;
        }
    }

    private async Task SaveCreateAsync()
    {
        if (_createModel is null) return;
        var validationError = ValidateWorkLog(_createModel, FilterState.IsAdmin);
        if (validationError is not null) { Crud.SetClientModalError(validationError); return; }

        await Crud.RunCrudAsync<WorkLogResponseDto>(
            operation: () => WorkLogApiClient.CreateAsync(_createModel),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: _ => {});
        if (Crud.ModalError is null)
        {
            try
            {
                await LoadWorklogsAsync();
                CloseCreateModal();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore nel reload post-CRUD: {ex}");
            }
        }
    }
}
