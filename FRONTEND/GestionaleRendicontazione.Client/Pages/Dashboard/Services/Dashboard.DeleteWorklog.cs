using GestionaleRendicontazione.Client.Models;
using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Dashboard;

// Logica della modale di conferma eliminazione worklog.
// Lo stato UI (IsSaving/ModalError) vive in CrudPageService; qui restano solo
// lo stato locale del modale (target + flag open) e la chiamata API specifica
// di WorkLogApiClient.DeleteAsync.
public partial class Dashboard
{
    private bool _deleteModalOpen;
    private WorkLogResponseDto? _deleteTarget;

    private void ConfirmDelete(WorkLogResponseDto worklog)
    {
        _deleteTarget = worklog;
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
            operation: () => WorkLogApiClient.DeleteAsync(id),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: () => {  });

        if (deleted)
        {
            try
            {
                await LoadWorklogsAsync();
                CloseDeleteModal();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore nel reload post-CRUD: {ex}");
            }
        }
    }
}
