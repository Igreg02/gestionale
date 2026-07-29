using GestionaleRendicontazione.Client.Services;
using Microsoft.AspNetCore.Components;

namespace GestionaleRendicontazione.Client.Pages.Admin;

public partial class Employees : ComponentBase, IDisposable
{
    [Inject] private CrudPageService Crud { get; set; } = default!;

    private List<EmployeeResponse> _employees = new();

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
            errorMessage: "Impossibile recuperare i dipendenti dal server. Riprova più tardi.");
    }

    private async Task ReloadListAsync()
    {
        _employees = (await EmployeeApiClient.GetAllAsync()).OrderBy(e => e.Username).ToList();
    }

    internal void ApplySaved(EmployeeResponse saved)
    {
        var idx = _employees.FindIndex(e => e.Id == saved.Id);
        if (idx >= 0) _employees[idx] = saved;
        else _employees.Add(saved);
        _employees = _employees.OrderBy(e => e.Username).ToList();
    }

    internal void ApplyRemoved(Guid id)
    {
        _employees.RemoveAll(e => e.Id == id);
    }
}