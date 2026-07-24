using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Dashboard;

// Logica della modale di conferma eliminazione worklog.
public partial class Dashboard
{
    private bool _deleteModalOpen;
    private WorkLogResponseDto? _deleteTarget;

    private void ConfirmDelete(WorkLogResponseDto worklog)
    {
        _deleteTarget = worklog;
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
            var result = await WorkLogApiClient.DeleteAsync(_deleteTarget.Id);
            if (!result.IsSuccess)
            {
                _modalError = result.ToUserMessage("Impossibile eliminare il worklog. Riprova più tardi.");
                return;
            }

            _worklogs.RemoveAll(w => w.Id == _deleteTarget.Id);
            CloseDeleteModal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore DELETE worklog: {ex}");
            _modalError = "Errore di rete. Riprova più tardi.";
        }
        finally
        {
            _isSaving = false;
        }
    }
}
