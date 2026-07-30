using GestionaleRendicontazione.Client.Services;
using Microsoft.AspNetCore.Components;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Questa classe è suddivisa in più file (partial) per responsabilità:
//  - Companies.razor.cs  -> stato condiviso, ciclo di vita, caricamento aziende
//  - Companies.Form.cs   -> modale "Nuova/Modifica azienda"
//  - Companies.Delete.cs -> modale conferma eliminazione
//
// Lo stato UI (IsLoading/IsSaving/ModalError/ErrorMessage) è centralizzato in
// CrudPageService — qui rimane solo la lista _companies e il ciclo di vita Blazor.
public partial class Companies : ComponentBase, IDisposable
{
    [Inject] private CrudPageService Crud { get; set; } = default!;

    private List<CompanyResponse> _companies = new();

    protected override void OnInitialized()
    {
        Crud.OnChanged += OnCrudStateChanged;
        _ = LoadAsync();
    }

    private void OnCrudStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => Crud.OnChanged -= OnCrudStateChanged;

    private async Task LoadAsync()
    {
        await Crud.RunLoadAsync(
            load: () => ReloadListAsync(),
            errorMessage: "Impossibile recuperare le aziende dal server. Riprova più tardi.");
    }

    private async Task ReloadListAsync()
    {
        _companies = (await CompanyApiClient.GetAllAsync()).OrderBy(c => c.Name).ToList();
    }
}