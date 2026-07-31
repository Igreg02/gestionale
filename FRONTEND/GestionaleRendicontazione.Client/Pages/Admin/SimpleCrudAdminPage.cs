using GestionaleRendicontazione.Client.Services;
using Microsoft.AspNetCore.Components;

namespace GestionaleRendicontazione.Client.Pages.Admin;

/// <summary>
/// Scheletro comune alle pagine Admin "entità semplice con solo Nome" (Companies, Statuses,
/// Types, ...): lista + modale crea/modifica + modale elimina, tutte orchestrate da
/// <see cref="CrudPageService"/>. Prima di questa base, Statuses/Types la ripetevano
/// identica in 3 file ciascuno (razor.cs + Form.cs + Delete.cs), differendo solo per i
/// tipi concreti (DTO/ApiClient/FormModel) — vedi audit codebase 2026-07-31.
///
/// Ogni pagina concreta implementa solo i 4 adapter verso il proprio ApiClient (@inject
/// nel .razor, come oggi) e le 4 conversioni DTO/FormModel specifiche dell'entità.
/// </summary>
public abstract class SimpleCrudAdminPage<TResponse, TCreate, TUpdate, TFormModel>
    : ComponentBase, IDisposable
    where TFormModel : new()
{
    [Inject] protected CrudPageService Crud { get; set; } = default!;
    [Inject] protected FilterStateService FilterState { get; set; } = default!;

    protected List<TResponse> _items = new();

    protected bool _formModalOpen;
    protected bool _isEditing;
    protected Guid _editingId;
    protected TFormModel? _formModel;

    protected bool _deleteModalOpen;
    protected TResponse? _deleteTarget;

    protected abstract string LoadErrorMessage { get; }

    protected abstract Task<List<TResponse>> FetchAllAsync();
    protected abstract Task<ApiResult<TResponse>> CreateAsync(TCreate dto);
    protected abstract Task<ApiResult<TResponse>> UpdateAsync(Guid id, TUpdate dto);
    protected abstract Task<ApiResult> DeleteAsync(Guid id);

    protected abstract Guid GetId(TResponse item);
    protected abstract TFormModel ToFormModel(TResponse item);
    protected abstract TCreate ToCreateDto(TFormModel model);
    protected abstract TUpdate ToUpdateDto(TFormModel model);

    /// <summary>Ritorna il messaggio di errore di validazione client-side, o null se valido.</summary>
    protected abstract string? Validate(TFormModel model);

    /// <summary>Ordinamento della lista dopo ogni reload. Default: nessun cambiamento.</summary>
    protected virtual List<TResponse> Sort(List<TResponse> items) => items;

    protected override void OnInitialized()
    {
        Crud.OnChanged += OnCrudStateChanged;
        _ = LoadAsync();
    }

    private void OnCrudStateChanged() => InvokeAsync(StateHasChanged);

    public void Dispose() => Crud.OnChanged -= OnCrudStateChanged;

    protected async Task LoadAsync()
    {
        await Crud.RunLoadAsync(load: ReloadListAsync, errorMessage: LoadErrorMessage);
    }

    protected async Task ReloadListAsync()
    {
        _items = Sort((await FetchAllAsync()).ToList());
    }

    protected void OpenCreateModal()
    {
        _isEditing = false;
        _editingId = Guid.Empty;
        _formModel = new TFormModel();
        Crud.ResetModalError();
        _formModalOpen = true;
    }

    protected void OpenEditModal(TResponse item)
    {
        _isEditing = true;
        _editingId = GetId(item);
        _formModel = ToFormModel(item);
        Crud.ResetModalError();
        _formModalOpen = true;
    }

    protected void CloseFormModal()
    {
        _formModalOpen = false;
        _formModel = default;
        Crud.ResetModalError();
    }

    protected async Task SaveFormAsync()
    {
        if (_formModel is null) return;

        var validationError = Validate(_formModel);
        if (validationError is not null)
        {
            Crud.SetClientModalError(validationError);
            return;
        }

        await Crud.RunCrudAsync<TResponse>(
            operation: _isEditing
                ? () => UpdateAsync(_editingId, ToUpdateDto(_formModel))
                : () => CreateAsync(ToCreateDto(_formModel)),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: _ => { });

        if (Crud.ModalError is null)
        {
            await AfterSaveOrDeleteAsync(CloseFormModal);
        }
    }

    protected void ConfirmDelete(TResponse item)
    {
        _deleteTarget = item;
        Crud.ResetModalError();
        _deleteModalOpen = true;
    }

    protected void CloseDeleteModal()
    {
        _deleteModalOpen = false;
        _deleteTarget = default;
        Crud.ResetModalError();
    }

    protected async Task ExecuteDeleteAsync()
    {
        if (_deleteTarget is null) return;

        var id = GetId(_deleteTarget);
        var deleted = await Crud.RunCrudAsync(
            operation: () => DeleteAsync(id),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: () => { });

        if (deleted)
        {
            await AfterSaveOrDeleteAsync(CloseDeleteModal);
        }
    }

    private async Task AfterSaveOrDeleteAsync(Action closeModal)
    {
        try
        {
            await ReloadListAsync();
            await FilterState.ReloadLookupsAsync();
            closeModal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nel reload post-CRUD: {ex}");
        }
    }
}
