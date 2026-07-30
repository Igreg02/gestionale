using GestionaleRendicontazione.Client.Services;
using Microsoft.AspNetCore.Components;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Questa classe è suddivisa in più file (partial) per responsabilità:
//  - Projects.razor.cs  -> stato condiviso, ciclo di vita, caricamento progetti/aziende
//  - Projects.Form.cs   -> modale "Nuovo/Modifica progetto"
//  - Projects.Delete.cs -> modale conferma eliminazione
//
// Lo stato UI (IsLoading/IsSaving/ModalError/ErrorMessage) è centralizzato in
// CrudPageService — qui rimane la lista _projects, la lookup _companies e il
// ciclo di vita Blazor.
public partial class Projects : ComponentBase, IDisposable
{
    [Inject] private CrudPageService Crud { get; set; } = default!;

    private List<ProjectResponse> _projects = new();
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
            load: () => ReloadListsAsync(),
            errorMessage: "Impossibile recuperare i progetti dal server. Riprova più tardi.");
    }

    private async Task ReloadListsAsync()
    {
        var projectsTask = ProjectApiClient.GetAllAsync();
        var companiesTask = CompanyApiClient.GetAllAsync();
        await Task.WhenAll(projectsTask, companiesTask);

        _projects = (await projectsTask).OrderBy(p => p.Name).ToList();
        _companies = (await companiesTask).OrderBy(c => c.Name).ToList();
    }
}