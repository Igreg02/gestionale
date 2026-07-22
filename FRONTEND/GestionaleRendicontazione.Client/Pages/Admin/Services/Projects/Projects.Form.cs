using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale "Nuovo/Modifica progetto".
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
        _modalError = null;
        _formModalOpen = true;
    }

    private void OpenEditModal(ProjectResponse project)
    {
        _isEditing = true;
        _editingId = project.Id;
        _formModel = new ProjectFormModel { Name = project.Name, IdCompany = project.IdCompany };
        _modalError = null;
        _formModalOpen = true;
    }

    private void CloseFormModal()
    {
        _formModalOpen = false;
        _formModel = null;
        _modalError = null;
    }

    private async Task SaveFormAsync()
    {
        if (_formModel is null) return;
        if (string.IsNullOrWhiteSpace(_formModel.Name)) { _modalError = "Il nome del progetto è obbligatorio."; return; }
        if (_formModel.IdCompany == Guid.Empty) { _modalError = "Seleziona un'azienda."; return; }

        _isSaving = true;
        _modalError = null;

        try
        {
            if (_isEditing)
            {
                var result = await ProjectApiClient.UpdateAsync(_editingId, new ProjectUpdateRequest { Name = _formModel.Name, IdCompany = _formModel.IdCompany });
                if (!result.IsSuccess) { _modalError = FormatError(result.ValidationErrors, result.ErrorMessage); return; }

                var idx = _projects.FindIndex(p => p.Id == _editingId);
                if (idx >= 0) _projects[idx] = result.Data!;
            }
            else
            {
                var result = await ProjectApiClient.CreateAsync(new ProjectCreateRequest { Name = _formModel.Name, IdCompany = _formModel.IdCompany });
                if (!result.IsSuccess) { _modalError = FormatError(result.ValidationErrors, result.ErrorMessage); return; }

                _projects.Add(result.Data!);
            }

            _projects = _projects.OrderBy(p => p.Name).ToList();
            _ = FilterState.ReloadLookupsAsync();
            CloseFormModal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nel salvataggio del progetto: {ex}");
            _modalError = "Errore di rete. Riprova più tardi.";
        }
        finally
        {
            _isSaving = false;
        }
    }
}

// Modello del form, esposto (non-privato) perché usato anche da ProjectFormModal.razor.
public sealed class ProjectFormModel
{
    public string Name { get; set; } = string.Empty;
    public Guid IdCompany { get; set; }
}
