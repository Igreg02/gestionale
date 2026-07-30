using GestionaleRendicontazione.Client.Services;
using Microsoft.AspNetCore.Components;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Questa classe è suddivisa in più file (partial) per responsabilità:
//  - Statuses.razor.cs  -> stato condiviso, ciclo di vita, caricamento stati
//  - Statuses.Form.cs   -> modale "Nuovo/Modifica stato"
//  - Statuses.Delete.cs -> modale conferma eliminazione
//
// Lo stato UI (IsLoading/IsSaving/ModalError/ErrorMessage) è centralizzato in
// CrudPageService — qui rimane solo la lista _items e il ciclo di vita Blazor.
public partial class Statuses : ComponentBase, IDisposable
{
    [Inject] private CrudPageService Crud { get; set; } = default!;

    private List<StatusResponse> _items = new();

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
            errorMessage: "Impossibile recuperare i dati dal server. Riprova più tardi.");
    }

    private async Task ReloadListAsync()
    {
        _items = (await ApiClient.GetAllAsync()).OrderBy(i => i.Name).ToList();
    }
}
