using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale di conferma eliminazione tipologia.
// Lo stato UI (IsSaving/ModalError) vive in CrudPageService; qui ci sono solo
// lo stato locale del modale (target + flag open) e la chiamata API specifica
// di TypeApiClient.DeleteAsync.
public partial class Types
{
    private bool _deleteModalOpen;
    private WorkTypeResponse? _deleteTarget;

    private void ConfirmDelete(WorkTypeResponse item)
    {
        _deleteTarget = item;
        Crud.ResetModalError();
        _deleteModalOpen = true;
    }

    private void CloseDeleteModal()
    {
        _deleteModalOpen = false;
        _deleteTarget = null;
        Crud.ResetModalError();
    }

    private async Task ExecuteDeleteAsync()
    {
        if (_deleteTarget is null) return;

        var id = _deleteTarget.Id;
        await Crud.RunCrudAsync(
            operation: () => ApiClient.DeleteAsync(id),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: () =>
            {
                ApplyRemoved(id);
                _ = FilterState.ReloadLookupsAsync();
                CloseDeleteModal();
            });
    }
}