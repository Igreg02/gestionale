using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

public partial class Employees
{
    private bool _deleteModalOpen;
    private EmployeeResponse? _deleteTarget;

    private void ConfirmDelete(EmployeeResponse employee)
    {
        _deleteTarget = employee;
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
            operation: () => EmployeeApiClient.DeleteAsync(id),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: () =>
            {
                ApplyRemoved(id);
                _ = FilterState.ReloadLookupsAsync();
                CloseDeleteModal();
            });
    }
}