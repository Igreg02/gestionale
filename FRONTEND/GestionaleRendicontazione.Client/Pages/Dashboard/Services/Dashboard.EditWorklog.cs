using System.Globalization;
using Microsoft.AspNetCore.Components;
using GestionaleRendicontazione.Client.Models;
using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Dashboard;

// Logica della modale "Modifica Worklog".
// Lo stato UI (IsSaving/ModalError) vive in CrudPageService; qui restano solo
// lo stato locale del form (modello, source originale, data string, flag
// modal-open) e la chiamata API specifica di WorkLogApiClient.UpdateAsync.
public partial class Dashboard
{
    private bool _editModalOpen;
    private WorkLogResponseDto? _editSource;
    private WorkLogUpdateRequestDto? _editModel;
    private string _editDateString = string.Empty;

    private async Task OpenEditModal(WorkLogResponseDto worklog)
    {
        _editSource = worklog;
        Crud.ResetModalError();
        if (FilterState.IsAdmin && _employees.Count == 0)
        {
            await LoadEmployeesAsync();
        }
        _editModel = new WorkLogUpdateRequestDto
        {
            Description = worklog.Description,
            HoursCounter = worklog.HoursCounter,
            Date = worklog.Date,
            IdProject = worklog.IdProject,
            IdEmployee = worklog.IdEmployee ?? Guid.Empty,
            IdType = worklog.IdType,
            IdStatus = worklog.IdStatus,
        };
        _editDateString = worklog.Date.ToString("yyyy-MM-dd");
        _editModalOpen = true;
    }

    private void CloseEditModal()
    {
        _editModalOpen = false;
        _editSource = null;
        _editModel = null;
        Crud.ResetModalError();
    }

    private void OnEditDateInput(ChangeEventArgs e)
    {
        _editDateString = e.Value?.ToString() ?? string.Empty;
        if (_editModel is not null && DateOnly.TryParseExact(_editDateString, "yyyy-MM-dd", null, DateTimeStyles.None, out var parsed))
        {
            _editModel.Date = parsed;
        }
    }

    private async Task SaveEditAsync()
    {
        if (_editModel is null || _editSource is null) return;
        var validationError = ValidateWorkLog(_editModel, FilterState.IsAdmin);
        if (validationError is not null) { Crud.SetClientModalError(validationError); return; }

        if (!FilterState.IsAdmin)
            _editModel.IdEmployee = _editSource.IdEmployee ?? Guid.Empty;

        var sourceId = _editSource.Id;
        await Crud.RunCrudAsync<WorkLogResponseDto>(
            operation: () => WorkLogApiClient.UpdateAsync(sourceId, _editModel),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: _ => {  });


        if (Crud.ModalError is null)
        {
            try
            {
                await LoadWorklogsAsync();
                CloseEditModal();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore nel reload post-CRUD: {ex}");
            }
        }
    }
}
