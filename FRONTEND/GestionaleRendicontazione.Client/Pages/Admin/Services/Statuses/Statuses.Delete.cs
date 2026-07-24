using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale di conferma eliminazione stato.
public partial class Statuses
{
    private bool _deleteModalOpen;
    private StatusResponse? _deleteTarget;

    private void ConfirmDelete(StatusResponse item)
    {
        _deleteTarget = item;
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
            var result = await ApiClient.DeleteAsync(_deleteTarget.Id);
            if (!result.IsSuccess) { _modalError = FormatError(result.ValidationErrors, result.ErrorMessage, result.StatusCode); return; }

            _items.RemoveAll(i => i.Id == _deleteTarget.Id);
            _ = FilterState.ReloadLookupsAsync();
            CloseDeleteModal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nell'eliminazione: {ex}");
            _modalError = "Errore di rete. Riprova più tardi.";
        }
        finally
        {
            _isSaving = false;
        }
    }
}
