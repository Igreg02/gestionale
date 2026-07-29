using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Dashboard;

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
        await Crud.RunCrudAsync(
            operation: () => WorkLogApiClient.DeleteAsync(id),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: () =>
            {
                _worklogs.RemoveAll(w => w.Id == id);
                CloseDeleteModal();
            });
    }
}
