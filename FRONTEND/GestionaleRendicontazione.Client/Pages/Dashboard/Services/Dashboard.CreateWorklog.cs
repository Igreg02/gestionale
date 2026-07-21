using System.Globalization;
using Microsoft.AspNetCore.Components;
using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Dashboard;

// Logica della modale "Nuovo Worklog".
public partial class Dashboard
{
    private bool _createModalOpen;
    private WorkLogUpdateRequestDto? _createModel;
    private string _createDateString = string.Empty;

    private async Task OpenCreateModal()
    {
        _modalError = null;
        if (FilterState.IsAdmin && _employees.Count == 0)
        {
            await LoadEmployeesAsync();
        }
        var today = DateOnly.FromDateTime(DateTime.Today);
        // Per l'utente non-admin il dipendente è sé stesso: lo pre-popoliamo con il proprio ID
        // ricavato dai claim (NameIdentifier = id utente). L'admin invece sceglie liberamente.
        var defaultEmployeeId = FilterState.IsAdmin
            ? Guid.Empty
            : GetCurrentUserId();
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
        _modalError = null;
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
        if (string.IsNullOrWhiteSpace(_createModel.Description)) { _modalError = "La descrizione è obbligatoria."; return; }
        if (_createModel.HoursCounter < 1 || _createModel.HoursCounter > 24) { _modalError = "Le ore devono essere comprese tra 1 e 24."; return; }
        if (_createModel.IdProject == Guid.Empty) { _modalError = "Seleziona un progetto."; return; }
        if (_createModel.IdType == Guid.Empty) { _modalError = "Seleziona una tipologia."; return; }
        if (_createModel.IdStatus == Guid.Empty) { _modalError = "Seleziona uno stato."; return; }
        if (FilterState.IsAdmin && _createModel.IdEmployee == Guid.Empty) { _modalError = "Seleziona un dipendente."; return; }

        _isSaving = true;
        _modalError = null;

        try
        {
            var result = await WorkLogApiClient.CreateAsync(_createModel);
            if (!result.IsSuccess)
            {
                _modalError = result.ToUserMessage("Errore durante la creazione del worklog.");
                return;
            }

            if (result.Data is not null)
                _worklogs.Insert(0, result.Data);
            CloseCreateModal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore POST worklog: {ex}");
            _modalError = "Errore di rete. Riprova più tardi.";
        }
        finally
        {
            _isSaving = false;
        }
    }
}
