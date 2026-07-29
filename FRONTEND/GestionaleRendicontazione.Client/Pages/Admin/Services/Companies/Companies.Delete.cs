using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

public partial class Companies
{
    private bool _deleteModalOpen;
    private CompanyResponse? _deleteTarget;

    private void ConfirmDelete(CompanyResponse company)
    {
        _deleteTarget = company;
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
            operation: () => CompanyApiClient.DeleteAsync(id),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: () =>
            {
                ApplyRemoved(id);
                _ = FilterState.ReloadLookupsAsync();
                CloseDeleteModal();
            });
    }
}