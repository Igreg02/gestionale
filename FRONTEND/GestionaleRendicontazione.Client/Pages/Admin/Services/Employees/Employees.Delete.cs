using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale di conferma eliminazione dipendente.
// Lo stato UI (IsSaving/ModalError) vive in CrudPageService; qui ci sono solo
// lo stato locale del modale (target + flag open) e la chiamata API specifica
// di EmployeeApiClient.DeleteAsync.
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
        var deleted = await Crud.RunCrudAsync(
            operation: () => EmployeeApiClient.DeleteAsync(id),
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