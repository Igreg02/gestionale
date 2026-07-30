using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale "Nuovo/Modifica progetto".
// Lo stato UI (IsSaving/ModalError) vive in CrudPageService; qui ci sono solo
// lo stato locale del form (modello + flag modal-open) e la chiamata API
// specifica di ProjectApiClient.
public partial class Projects
{
    private bool _formModalOpen;
    private bool _isEditing;
    private Guid _editingId;
    private ProjectFormModel? _formModel;

    private void OpenCreateModal()
    {
        _isEditing = false;
        _editingId = Guid.Empty;
        _formModel = new ProjectFormModel();
        Crud.ResetModalError();
        _formModalOpen = true;
    }

    private void OpenEditModal(ProjectResponse project)
    {
        _isEditing = true;
        _editingId = project.Id;
        _formModel = new ProjectFormModel { Name = project.Name, IdCompany = project.IdCompany };
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
            Crud.SetClientModalError("Il nome del progetto è obbligatorio.");
            return;
        }
        if (_formModel.IdCompany == Guid.Empty)
        {
            Crud.SetClientModalError("Seleziona un'azienda.");
            return;
        }

        await Crud.RunCrudAsync<ProjectResponse>(
            operation: _isEditing
                ? () => ProjectApiClient.UpdateAsync(_editingId, new ProjectUpdateRequest { Name = _formModel.Name, IdCompany = _formModel.IdCompany })
                : () => ProjectApiClient.CreateAsync(new ProjectCreateRequest { Name = _formModel.Name, IdCompany = _formModel.IdCompany }),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: _ => { });

        if (Crud.ModalError is null)
        {
            try
            {
                await ReloadListsAsync();
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

// Modello del form, esposto (non-privato) perché usato anche da ProjectFormModal.razor.
public sealed class ProjectFormModel
{
    public string Name { get; set; } = string.Empty;
    public Guid IdCompany { get; set; }
}