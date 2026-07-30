using GestionaleRendicontazione.Client.Services;
using Microsoft.AspNetCore.Components;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Questa classe è suddivisa in più file (partial) per responsabilità:
//  - Employees.razor.cs     -> stato condiviso, ciclo di vita, caricamento dipendenti
//  - Employees.Register.cs  -> modale "Nuovo dipendente" (POST /api/auth/register)
//  - Employees.Edit.cs      -> modale "Modifica dipendente"
//  - Employees.Delete.cs    -> modale conferma eliminazione
//
// Lo stato UI (IsLoading/IsSaving/ModalError/ErrorMessage) è centralizzato in
// CrudPageService — qui rimane solo la lista _employees e il ciclo di vita Blazor.
// L'unica eccezione è la modale di Register, che usa il proprio _registerError /
// _isRegistering perché non passa dal flow CRUD standard (usa AuthService.RegisterAsync).
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
}