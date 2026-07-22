using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale di conferma eliminazione dipendente.
public partial class Employees
{
    private bool _deleteModalOpen;
    private EmployeeResponse? _deleteTarget;

    private void ConfirmDelete(EmployeeResponse employee)
    {
        _deleteTarget = employee;
        _modalError = null;
        _deleteModalOpen = true;
    }

    private void CloseDeleteModal()
    {
        _deleteModalOpen = false;
        _deleteTarget = null;
        _modalError = null;
    }

    private async Task ExecuteDeleteAsync()
    {
        if (_deleteTarget is null) return;

        _isSaving = true;
        _modalError = null;

        try
        {
            var result = await EmployeeApiClient.DeleteAsync(_deleteTarget.Id);
            if (!result.IsSuccess) { _modalError = FormatError(result.ValidationErrors, result.ErrorMessage); return; }

            _employees.RemoveAll(e => e.Id == _deleteTarget.Id);
            _ = FilterState.ReloadLookupsAsync();
            CloseDeleteModal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nell'eliminazione del dipendente: {ex}");
            _modalError = "Errore di rete. Riprova più tardi.";
        }
        finally
        {
            _isSaving = false;
        }
    }
}
