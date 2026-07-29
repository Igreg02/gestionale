using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

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
            onSuccess: saved =>
            {
                ApplySaved(saved);
                _ = FilterState.ReloadLookupsAsync();
                CloseFormModal();
            });
    }
}

public sealed class ProjectFormModel
{
    public string Name { get; set; } = string.Empty;
    public Guid IdCompany { get; set; }
}