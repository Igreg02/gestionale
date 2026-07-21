using System.Globalization;
using Microsoft.AspNetCore.Components;
using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Dashboard;

// Logica della modale "Modifica Worklog".
public partial class Dashboard
{
    private bool _editModalOpen;
    private WorkLogResponseDto? _editSource;
    private WorkLogUpdateRequestDto? _editModel;
    private string _editDateString = string.Empty;

    private async Task OpenEditModal(WorkLogResponseDto worklog)
    {
        _editSource = worklog;
        _modalError = null;
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
        _modalError = null;
    }

    private void OnDateInput(ChangeEventArgs e)
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
        if (string.IsNullOrWhiteSpace(_editModel.Description)) { _modalError = "La descrizione è obbligatoria."; return; }
        if (_editModel.HoursCounter < 1 || _editModel.HoursCounter > 24) { _modalError = "Le ore devono essere comprese tra 1 e 24."; return; }
        if (_editModel.IdProject == Guid.Empty) { _modalError = "Seleziona un progetto."; return; }
        if (_editModel.IdType == Guid.Empty) { _modalError = "Seleziona una tipologia."; return; }
        if (_editModel.IdStatus == Guid.Empty) { _modalError = "Seleziona uno stato."; return; }
        if (FilterState.IsAdmin && _editModel.IdEmployee == Guid.Empty) { _modalError = "Seleziona un dipendente."; return; }

        if (!FilterState.IsAdmin)
            _editModel.IdEmployee = _editSource.IdEmployee ?? Guid.Empty;

        _isSaving = true;
        _modalError = null;

        try
        {
            var updateResult = await WorkLogApiClient.UpdateAsync(_editSource.Id, _editModel);
            if (!updateResult.IsSuccess)
            {
                _modalError = updateResult.ToUserMessage("Errore durante il salvataggio. Riprova più tardi.");
                return;
            }

            var updated = updateResult.Data;
            if (updated is null) { _modalError = "Errore durante il salvataggio. Riprova più tardi."; return; }

            var idx = _worklogs.FindIndex(w => w.Id == updated.Id);
            if (idx >= 0) _worklogs[idx] = updated;

            CloseEditModal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore PUT worklog: {ex}");
            _modalError = "Errore di rete. Riprova più tardi.";
        }
        finally
        {
            _isSaving = false;
        }
    }
}
