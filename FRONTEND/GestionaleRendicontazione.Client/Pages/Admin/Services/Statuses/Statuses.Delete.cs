using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale di conferma eliminazione stato.
// Lo stato UI (IsSaving/ModalError) vive in CrudPageService; qui ci sono solo
// lo stato locale del modale (target + flag open) e la chiamata API specifica
// di StatusApiClient.DeleteAsync.
public partial class Statuses
{
    private bool _deleteModalOpen;
    private StatusResponse? _deleteTarget;

    private void ConfirmDelete(StatusResponse item)
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
        var deleted = await Crud.RunCrudAsync(
            operation: () => ApiClient.DeleteAsync(id),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: () => {  });

        if (deleted)
        {
            try
            {
                await ReloadListAsync();
                await FilterState.ReloadLookupsAsync();
                CloseDeleteModal();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore nel reload post-CRUD: {ex}");
            }
        }
    }
}