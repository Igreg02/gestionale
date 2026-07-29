using GestionaleRendicontazione.Client.Services;
using Microsoft.AspNetCore.Components;

namespace GestionaleRendicontazione.Client.Pages.Admin;

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

    internal void ApplySaved(ProjectResponse saved)
    {
        var idx = _projects.FindIndex(p => p.Id == saved.Id);
        if (idx >= 0) _projects[idx] = saved;
        else _projects.Add(saved);
        _projects = _projects.OrderBy(p => p.Name).ToList();
    }

    internal void ApplyRemoved(Guid id)
    {
        _projects.RemoveAll(p => p.Id == id);
    }
}