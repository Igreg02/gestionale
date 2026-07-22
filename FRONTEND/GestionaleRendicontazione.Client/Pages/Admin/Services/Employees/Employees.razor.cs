using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Questa classe è suddivisa in più file (partial) per responsabilità:
//  - Employees.razor.cs  -> stato condiviso, ciclo di vita, caricamento dipendenti
//  - Employees.Register.cs -> modale "Nuovo dipendente" (POST /api/auth/register)
//  - Employees.Edit.cs   -> modale "Modifica dipendente"
//  - Employees.Delete.cs -> modale conferma eliminazione
public partial class Employees
{
    private bool _loading = true;
    private string? _errorMessage;
    private List<EmployeeResponse> _employees = new();

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
            _employees = (await EmployeeApiClient.GetAllAsync()).OrderBy(e => e.Username).ToList();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nel recupero dei dipendenti: {ex}");
            _errorMessage = "Impossibile recuperare i dipendenti dal server. Riprova più tardi.";
        }
        finally
        {
            _loading = false;
        }
    }

    private static string FormatError(Dictionary<string, string[]> validationErrors, string? errorMessage)
    {
        if (validationErrors.Count > 0)
            return string.Join(" ", validationErrors.SelectMany(kv => kv.Value));
        return errorMessage ?? "Si è verificato un errore imprevisto.";
    }
}
