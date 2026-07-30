using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale "Nuovo/Modifica stato".
// Lo stato UI (IsSaving/ModalError) vive in CrudPageService; qui ci sono solo
// lo stato locale del form (modello + flag modal-open) e la chiamata API
// specifica di StatusApiClient.
public partial class Statuses
{
    private bool _formModalOpen;
    private bool _isEditing;
    private Guid _editingId;
    private StatusFormModel? _formModel;

    private void OpenCreateModal()
    {
        _isEditing = false;
        _editingId = Guid.Empty;
        _formModel = new StatusFormModel();
        Crud.ResetModalError();
        _formModalOpen = true;
    }

    private void OpenEditModal(StatusResponse item)
    {
        _isEditing = true;
        _editingId = item.Id;
        _formModel = new StatusFormModel { Name = item.Name };
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

        await Crud.RunCrudAsync<StatusResponse>(
            operation: _isEditing
                ? () => ApiClient.UpdateAsync(_editingId, new StatusUpdateRequest { Name = _formModel.Name })
                : () => ApiClient.CreateAsync(new StatusCreateRequest { Name = _formModel.Name }),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: _ => {  });

        if (Crud.ModalError is null)
        {
            try
            {
                await ReloadListAsync();
                await FilterState.ReloadLookupsAsync();
                CloseFormModal();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore nel reload post-CRUD: {ex}");
            }
        }
    }
}

// Modello del form, esposto (non-privato) perché usato anche da StatusFormModal.razor.
public sealed class StatusFormModel
{
    public string Name { get; set; } = string.Empty;
}
