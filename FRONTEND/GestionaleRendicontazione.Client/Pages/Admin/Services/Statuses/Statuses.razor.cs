using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Questa classe è suddivisa in più file (partial) per responsabilità:
//  - Statuses.razor.cs  -> stato condiviso, ciclo di vita, caricamento stati
//  - Statuses.Form.cs   -> modale "Nuovo/Modifica stato"
//  - Statuses.Delete.cs -> modale conferma eliminazione
public partial class Statuses
{
    private bool _loading = true;
    private string? _errorMessage;
    private List<StatusResponse> _items = new();

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
            _items = (await ApiClient.GetAllAsync()).OrderBy(i => i.Name).ToList();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nel recupero dei dati: {ex}");
            _errorMessage = "Impossibile recuperare i dati dal server. Riprova più tardi.";
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
