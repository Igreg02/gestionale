using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale "Nuovo/Modifica tipologia".
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
        _modalError = null;
        _formModalOpen = true;
    }

    private void OpenEditModal(WorkTypeResponse item)
    {
        _isEditing = true;
        _editingId = item.Id;
        _formModel = new TypeFormModel { Name = item.Name };
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
        if (string.IsNullOrWhiteSpace(_formModel.Name)) { _modalError = "Il nome è obbligatorio."; return; }

        _isSaving = true;
        _modalError = null;

        try
        {
            if (_isEditing)
            {
                var result = await ApiClient.UpdateAsync(_editingId, new WorkTypeUpdateRequest { Name = _formModel.Name });
                if (!result.IsSuccess) { _modalError = FormatError(result.ValidationErrors, result.ErrorMessage, result.StatusCode); return; }

                var idx = _items.FindIndex(i => i.Id == _editingId);
                if (idx >= 0) _items[idx] = result.Data!;
            }
            else
            {
                var result = await ApiClient.CreateAsync(new WorkTypeCreateRequest { Name = _formModel.Name });
                if (!result.IsSuccess) { _modalError = FormatError(result.ValidationErrors, result.ErrorMessage, result.StatusCode); return; }

                _items.Add(result.Data!);
            }

            _items = _items.OrderBy(i => i.Name).ToList();
            _ = FilterState.ReloadLookupsAsync();
            CloseFormModal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nel salvataggio: {ex}");
            _modalError = "Errore di rete. Riprova più tardi.";
        }
        finally
        {
            _isSaving = false;
        }
    }
}

// Modello del form, esposto (non-privato) perché usato anche da TypeFormModal.razor.
public sealed class TypeFormModel
{
    public string Name { get; set; } = string.Empty;
}
