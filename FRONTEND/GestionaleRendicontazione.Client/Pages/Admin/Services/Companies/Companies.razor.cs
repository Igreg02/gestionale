using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Questa classe è suddivisa in più file (partial) per responsabilità:
//  - Companies.razor.cs  -> stato condiviso, ciclo di vita, caricamento aziende
//  - Companies.Form.cs   -> modale "Nuova/Modifica azienda"
//  - Companies.Delete.cs -> modale conferma eliminazione
public partial class Companies
{
    private bool _loading = true;
    private string? _errorMessage;
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
            _companies = (await CompanyApiClient.GetAllAsync())
                .OrderBy(c => c.Name)
                .ToList();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nel recupero delle aziende: {ex}");
            _errorMessage = "Impossibile recuperare le aziende dal server. Riprova più tardi.";
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
