using GestionaleRendicontazione.Client.Models;
using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Tutta la logica CRUD (load/create/update/delete + stato dei due modali) vive nella
// base SimpleCrudAdminPage: qui restano solo gli adapter verso StatusApiClient (@inject
// nel .razor) e le conversioni DTO/FormModel specifiche di questa entità.
public partial class Statuses
{
    protected override string LoadErrorMessage => "Impossibile recuperare i dati dal server. Riprova più tardi.";

    protected override Task<List<StatusResponse>> FetchAllAsync() => ApiClient.GetAllAsync();
    protected override Task<ApiResult<StatusResponse>> CreateAsync(StatusCreateRequest dto) => ApiClient.CreateAsync(dto);
    protected override Task<ApiResult<StatusResponse>> UpdateAsync(Guid id, StatusUpdateRequest dto) => ApiClient.UpdateAsync(id, dto);
    protected override Task<ApiResult> DeleteAsync(Guid id) => ApiClient.DeleteAsync(id);

    protected override List<StatusResponse> Sort(List<StatusResponse> items) => items.OrderBy(i => i.Name).ToList();

    protected override Guid GetId(StatusResponse item) => item.Id;
    protected override StatusFormModel ToFormModel(StatusResponse item) => new() { Name = item.Name };
    protected override StatusCreateRequest ToCreateDto(StatusFormModel model) => new() { Name = model.Name };
    protected override StatusUpdateRequest ToUpdateDto(StatusFormModel model) => new() { Name = model.Name };

    protected override string? Validate(StatusFormModel model)
        => string.IsNullOrWhiteSpace(model.Name) ? "Il nome è obbligatorio." : null;
}

// Modello del form, esposto (non-privato) perché usato anche da StatusFormModal.razor.
public sealed class StatusFormModel
{
    public string Name { get; set; } = string.Empty;
}
