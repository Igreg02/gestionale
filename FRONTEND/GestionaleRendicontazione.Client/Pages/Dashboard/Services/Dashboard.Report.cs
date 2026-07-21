namespace GestionaleRendicontazione.Client.Pages.Dashboard;

// Apertura/chiusura della modale di generazione report.
public partial class Dashboard
{
    private bool _reportModalOpen;

    private void OpenReportModal()  => _reportModalOpen = true;

    private void CloseReportModal() => _reportModalOpen = false;
}
