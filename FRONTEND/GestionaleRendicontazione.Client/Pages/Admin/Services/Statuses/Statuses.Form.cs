using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale "Nuovo/Modifica stato".
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
        _modalError = null;
        _formModalOpen = true;
    }

    private void OpenEditModal(StatusResponse item)
    {
        _isEditing = true;
        _editingId = item.Id;
        _formModel = new StatusFormModel { Name = item.Name };
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
                var result = await ApiClient.UpdateAsync(_editingId, new StatusUpdateRequest { Name = _formModel.Name });
                if (!result.IsSuccess) { _modalError = FormatError(result.ValidationErrors, result.ErrorMessage, result.StatusCode); return; }

                var idx = _items.FindIndex(i => i.Id == _editingId);
                if (idx >= 0) _items[idx] = result.Data!;
            }
            else
            {
                var result = await ApiClient.CreateAsync(new StatusCreateRequest { Name = _formModel.Name });
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

// Modello del form, esposto (non-privato) perché usato anche da StatusFormModal.razor.
public sealed class StatusFormModel
{
    public string Name { get; set; } = string.Empty;
}
