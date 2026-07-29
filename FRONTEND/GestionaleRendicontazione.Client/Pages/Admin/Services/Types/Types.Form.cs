using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale "Nuovo/Modifica tipologia".
// Lo stato UI (IsSaving/ModalError) vive in CrudPageService; qui ci sono solo
// lo stato locale del form (modello + flag modal-open) e la chiamata API
// specifica di TypeApiClient.
public partial class Types
{
    private bool _formModalOpen;
    private bool _isEditing;
    private Guid _editingId;
    private TypeFormModel? _formModel;

    private void OpenCreateModal()
    {
        _isEditing = false;
        _editingId = Guid.Empty;
        _formModel = new TypeFormModel();
        Crud.ResetModalError();
        _formModalOpen = true;
    }

    private void OpenEditModal(WorkTypeResponse item)
    {
        _isEditing = true;
        _editingId = item.Id;
        _formModel = new TypeFormModel { Name = item.Name };
        Crud.ResetModalError();
        _formModalOpen = true;
    }

    private void CloseFormModal()
    {
        _formModalOpen = false;
        _formModel = null;
        Crud.ResetModalError();
    }

    private async Task SaveFormAsync()
    {
        if (_formModel is null) return;
        if (string.IsNullOrWhiteSpace(_formModel.Name))
        {
            Crud.SetClientModalError("Il nome è obbligatorio.");
            return;
        }

        await Crud.RunCrudAsync<WorkTypeResponse>(
            operation: _isEditing
                ? () => ApiClient.UpdateAsync(_editingId, new WorkTypeUpdateRequest { Name = _formModel.Name })
                : () => ApiClient.CreateAsync(new WorkTypeCreateRequest { Name = _formModel.Name }),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: saved =>
            {
                ApplySaved(saved);
                _ = FilterState.ReloadLookupsAsync();
                CloseFormModal();
            });
    }
}

// Modello del form, esposto (non-privato) perché usato anche da TypeFormModal.razor.
public sealed class TypeFormModel
{
    public string Name { get; set; } = string.Empty;
}