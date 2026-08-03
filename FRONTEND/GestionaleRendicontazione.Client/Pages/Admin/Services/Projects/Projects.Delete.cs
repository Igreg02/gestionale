using GestionaleRendicontazione.Client.Models;
using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale di conferma eliminazione progetto.
// Lo stato UI (IsSaving/ModalError) vive in CrudPageService; qui ci sono solo
// lo stato locale del modale (target + flag open) e la chiamata API specifica
// di ProjectApiClient.DeleteAsync.
public partial class Projects
{
    private bool _deleteModalOpen;
    private ProjectResponse? _deleteTarget;

    private void ConfirmDelete(ProjectResponse project)
    {
        _deleteTarget = project;
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
            operation: () => ProjectApiClient.DeleteAsync(id),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: () => { });

        if (deleted)
        {
            try
            {
                await ReloadListsAsync();
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