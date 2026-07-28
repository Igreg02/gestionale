using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale "Nuova/Modifica azienda".
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
        _modalError = null;
        _formModalOpen = true;
    }

    private void OpenEditModal(CompanyResponse company)
    {
        _isEditing = true;
        _editingId = company.Id;
        _formModel = new CompanyFormModel { Name = company.Name, Email = company.Email };
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

        if (string.IsNullOrWhiteSpace(_formModel.Name)) { _modalError = "Il nome dell'azienda è obbligatorio."; return; }
        if (string.IsNullOrWhiteSpace(_formModel.Email)) { _modalError = "L'email dell'azienda è obbligatoria."; return; }

        _isSaving = true;
        _modalError = null;

        try
        {
            if (_isEditing)
            {
                var result = await CompanyApiClient.UpdateAsync(_editingId, new CompanyUpdateRequest { Name = _formModel.Name, Email = _formModel.Email });
                if (!result.IsSuccess) { _modalError = FormatError(result.ValidationErrors, result.ErrorMessage, result.StatusCode); return; }

                var idx = _companies.FindIndex(c => c.Id == _editingId);
                if (idx >= 0) _companies[idx] = result.Data!;
            }
            else
            {
                var result = await CompanyApiClient.CreateAsync(new CompanyCreateRequest { Name = _formModel.Name, Email = _formModel.Email });
                if (!result.IsSuccess) { _modalError = FormatError(result.ValidationErrors, result.ErrorMessage, result.StatusCode); return; }

                _companies.Add(result.Data!);
            }

            _companies = _companies.OrderBy(c => c.Name).ToList();
            _ = FilterState.ReloadLookupsAsync();
            CloseFormModal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nel salvataggio dell'azienda: {ex}");
            _modalError = "Errore di rete. Riprova più tardi.";
        }
        finally
        {
            _isSaving = false;
        }
    }

}

// Modello del form, esposto (non-privato) perché usato anche da CompanyFormModal.razor.
public sealed class CompanyFormModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
