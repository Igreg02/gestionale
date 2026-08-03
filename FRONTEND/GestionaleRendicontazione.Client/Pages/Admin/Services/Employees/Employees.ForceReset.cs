using GestionaleRendicontazione.Client.Models;
using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale di conferma "Forza reset password" (riusa NamedItemDeleteModal
// con Icon="bi-key" — vedi Employees.razor). Lo stato UI (IsSaving/ModalError) vive in
// CrudPageService; qui ci sono solo lo stato locale del modale (target + flag open) e
// la chiamata API specifica di EmployeeApiClient.ForcePasswordResetAsync.
public partial class Employees
{
    private bool _forceResetModalOpen;
    private EmployeeResponse? _forceResetTarget;

    private void ConfirmForceReset(EmployeeResponse employee)
    {
        _forceResetTarget = employee;
        Crud.ResetModalError();
        _forceResetModalOpen = true;
    }

    private void CloseForceResetModal()
    {
        _forceResetModalOpen = false;
        _forceResetTarget = null;
        Crud.ResetModalError();
    }

    private async Task ExecuteForceResetAsync()
    {
        if (_forceResetTarget is null) return;

        var id = _forceResetTarget.Id;
        var done = await Crud.RunCrudAsync(
            operation: () => EmployeeApiClient.ForcePasswordResetAsync(id),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: () => { });

        if (done)
        {
            try
            {
                await ReloadListAsync();
                CloseForceResetModal();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore nel reload post-CRUD: {ex}");
            }
        }
    }
}
