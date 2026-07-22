using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale di conferma eliminazione azienda.
public partial class Companies
{
    private bool _deleteModalOpen;
    private CompanyResponse? _deleteTarget;

    private void ConfirmDelete(CompanyResponse company)
    {
        _deleteTarget = company;
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
            var result = await CompanyApiClient.DeleteAsync(_deleteTarget.Id);
            if (!result.IsSuccess) { _modalError = FormatError(result.ValidationErrors, result.ErrorMessage); return; }

            _companies.RemoveAll(c => c.Id == _deleteTarget.Id);
            _ = FilterState.ReloadLookupsAsync();
            CloseDeleteModal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nell'eliminazione dell'azienda: {ex}");
            _modalError = "Errore di rete. Riprova più tardi.";
        }
        finally
        {
            _isSaving = false;
        }
    }
}
