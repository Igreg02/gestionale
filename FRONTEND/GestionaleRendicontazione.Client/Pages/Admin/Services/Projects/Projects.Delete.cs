using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale di conferma eliminazione progetto.
public partial class Projects
{
    private bool _deleteModalOpen;
    private ProjectResponse? _deleteTarget;

    private void ConfirmDelete(ProjectResponse project)
    {
        _deleteTarget = project;
        _modalError = null;
        _deleteModalOpen = true;
    }

    private void CloseDeleteModal()
    {
        _deleteModalOpen = false;
        _deleteTarget = null;
        _modalError = null;
    }

    private async Task ExecuteDeleteAsync()
    {
        if (_deleteTarget is null) return;

        _isSaving = true;
        _modalError = null;

        try
        {
            var result = await ProjectApiClient.DeleteAsync(_deleteTarget.Id);
            if (!result.IsSuccess) { _modalError = FormatError(result.ValidationErrors, result.ErrorMessage); return; }

            _projects.RemoveAll(p => p.Id == _deleteTarget.Id);
            _ = FilterState.ReloadLookupsAsync();
            CloseDeleteModal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nell'eliminazione del progetto: {ex}");
            _modalError = "Errore di rete. Riprova più tardi.";
        }
        finally
        {
            _isSaving = false;
        }
    }
}
