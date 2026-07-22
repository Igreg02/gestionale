using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale "Modifica dipendente".
public partial class Employees
{
    private bool _formModalOpen;
    private Guid _editingId;
    private EmployeeFormModel? _formModel;

    private void OpenEditModal(EmployeeResponse employee)
    {
        _editingId = employee.Id;
        _formModel = new EmployeeFormModel
        {
            Username = employee.Username,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
        };
        _modalError = null;
        _formModalOpen = true;
    }

    private void CloseFormModal()
    {
        _formModalOpen = false;
        _formModel = null;
        _modalError = null;
    }

    private async Task SaveFormAsync()
    {
        if (_formModel is null) return;
        if (string.IsNullOrWhiteSpace(_formModel.Username)) { _modalError = "Lo username è obbligatorio."; return; }
        if (string.IsNullOrWhiteSpace(_formModel.FirstName)) { _modalError = "Il nome è obbligatorio."; return; }
        if (string.IsNullOrWhiteSpace(_formModel.LastName)) { _modalError = "Il cognome è obbligatorio."; return; }

        _isSaving = true;
        _modalError = null;

        try
        {
            var result = await EmployeeApiClient.UpdateAsync(_editingId, new EmployeeUpdateRequest
            {
                Username = _formModel.Username,
                FirstName = _formModel.FirstName,
                LastName = _formModel.LastName,
            });
            if (!result.IsSuccess) { _modalError = FormatError(result.ValidationErrors, result.ErrorMessage); return; }

            var idx = _employees.FindIndex(e => e.Id == _editingId);
            if (idx >= 0) _employees[idx] = result.Data!;

            _employees = _employees.OrderBy(e => e.Username).ToList();
            _ = FilterState.ReloadLookupsAsync();
            CloseFormModal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nel salvataggio del dipendente: {ex}");
            _modalError = "Errore di rete. Riprova più tardi.";
        }
        finally
        {
            _isSaving = false;
        }
    }
}

// Modello del form, esposto (non-privato) perché usato anche da EmployeeEditModal.razor.
public sealed class EmployeeFormModel
{
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
