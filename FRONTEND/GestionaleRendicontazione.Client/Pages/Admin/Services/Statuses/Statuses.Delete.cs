using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

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