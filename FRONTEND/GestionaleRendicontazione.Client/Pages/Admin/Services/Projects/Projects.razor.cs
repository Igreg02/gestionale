using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Questa classe è suddivisa in più file (partial) per responsabilità:
//  - Projects.razor.cs  -> stato condiviso, ciclo di vita, caricamento progetti/aziende
//  - Projects.Form.cs   -> modale "Nuovo/Modifica progetto"
//  - Projects.Delete.cs -> modale conferma eliminazione
public partial class Projects
{
    private bool _loading = true;
    private string? _errorMessage;
    private List<ProjectResponse> _projects = new();
    private List<CompanyResponse> _companies = new();

    private bool _isSaving;
    private string? _modalError;

    protected override async Task OnInitializedAsync() => await LoadAsync();

    private async Task LoadAsync()
    {
        _loading = true;
        _errorMessage = null;
        StateHasChanged();

        try
        {
            var projectsTask = ProjectApiClient.GetAllAsync();
            var companiesTask = CompanyApiClient.GetAllAsync();
            await Task.WhenAll(projectsTask, companiesTask);

            _projects = (await projectsTask).OrderBy(p => p.Name).ToList();
            _companies = (await companiesTask).OrderBy(c => c.Name).ToList();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nel recupero dei progetti: {ex}");
            _errorMessage = "Impossibile recuperare i progetti dal server. Riprova più tardi.";
        }
        finally
        {
            _loading = false;
        }
    }

    private static string FormatError(Dictionary<string, string[]> validationErrors, string? errorMessage, int statusCode = 0)
    {
        if (statusCode == 409)
            return ApiResultExtensions.ToUserMessage(statusCode, validationErrors, errorMessage, null);
        if (validationErrors.Count > 0)
            return string.Join(" ", validationErrors.SelectMany(kv => kv.Value));
        return errorMessage ?? "Si è verificato un errore imprevisto.";
    }
}
