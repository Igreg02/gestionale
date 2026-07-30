using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale "Nuova/Modifica azienda".
// Lo stato UI (IsSaving/ModalError) vive in CrudPageService; qui ci sono solo
// lo stato locale del form (modello + flag modal-open) e la chiamata API
// specifica di CompanyApiClient.
public partial class Companies
{
    private bool _formModalOpen;
    private bool _isEditing;
    private Guid _editingId;
    private CompanyFormModel? _formModel;

    private void OpenCreateModal()
    {
        _isEditing = false;
        _editingId = Guid.Empty;
        _formModel = new CompanyFormModel();
        Crud.ResetModalError();
        _formModalOpen = true;
    }

    private void OpenEditModal(CompanyResponse company)
    {
        _isEditing = true;
        _editingId = company.Id;
        _formModel = new CompanyFormModel { Name = company.Name, Email = company.Email };
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
            Crud.SetClientModalError("Il nome dell'azienda è obbligatorio.");
            return;
        }
        if (string.IsNullOrWhiteSpace(_formModel.Email))
        {
            Crud.SetClientModalError("L'email dell'azienda è obbligatoria.");
            return;
        }

        await Crud.RunCrudAsync<CompanyResponse>(
            operation: _isEditing
                ? () => CompanyApiClient.UpdateAsync(_editingId, new CompanyUpdateRequest { Name = _formModel.Name, Email = _formModel.Email })
                : () => CompanyApiClient.CreateAsync(new CompanyCreateRequest { Name = _formModel.Name, Email = _formModel.Email }),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: _ => { });

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

// Modello del form, esposto (non-privato) perché usato anche da CompanyFormModal.razor.
public sealed class CompanyFormModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}