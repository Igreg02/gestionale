using GestionaleRendicontazione.Client.Models;
using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Tutta la logica CRUD (load/create/update/delete + stato dei due modali) vive nella
// base SimpleCrudAdminPage: qui restano solo gli adapter verso TypeApiClient (@inject
// nel .razor) e le conversioni DTO/FormModel specifiche di questa entità.
public partial class Types
{
    protected override string LoadErrorMessage => "Impossibile recuperare i dati dal server. Riprova più tardi.";

    protected override Task<List<WorkTypeResponse>> FetchAllAsync() => ApiClient.GetAllAsync();
    protected override Task<ApiResult<WorkTypeResponse>> CreateAsync(WorkTypeCreateRequest dto) => ApiClient.CreateAsync(dto);
    protected override Task<ApiResult<WorkTypeResponse>> UpdateAsync(Guid id, WorkTypeUpdateRequest dto) => ApiClient.UpdateAsync(id, dto);
    protected override Task<ApiResult> DeleteAsync(Guid id) => ApiClient.DeleteAsync(id);

    protected override List<WorkTypeResponse> Sort(List<WorkTypeResponse> items) => items.OrderBy(i => i.Name).ToList();

    protected override Guid GetId(WorkTypeResponse item) => item.Id;
    protected override TypeFormModel ToFormModel(WorkTypeResponse item) => new() { Name = item.Name };
    protected override WorkTypeCreateRequest ToCreateDto(TypeFormModel model) => new() { Name = model.Name };
    protected override WorkTypeUpdateRequest ToUpdateDto(TypeFormModel model) => new() { Name = model.Name };

    protected override string? Validate(TypeFormModel model)
        => string.IsNullOrWhiteSpace(model.Name) ? "Il nome è obbligatorio." : null;
}

// Modello del form, esposto (non-privato) perché usato anche da TypeFormModal.razor.
public sealed class TypeFormModel
{
    public string Name { get; set; } = string.Empty;
}
