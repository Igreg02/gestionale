using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Tutta la logica CRUD (load/create/update/delete + stato dei due modali) vive nella
// base SimpleCrudAdminPage: qui restano solo gli adapter verso CompanyApiClient (@inject
// nel .razor, come ApiClient) e le conversioni DTO/FormModel specifiche di questa entità.
public partial class Companies
{
    protected override string LoadErrorMessage => "Impossibile recuperare le aziende dal server. Riprova più tardi.";

    protected override Task<List<CompanyResponse>> FetchAllAsync() => ApiClient.GetAllAsync();
    protected override Task<ApiResult<CompanyResponse>> CreateAsync(CompanyCreateRequest dto) => ApiClient.CreateAsync(dto);
    protected override Task<ApiResult<CompanyResponse>> UpdateAsync(Guid id, CompanyUpdateRequest dto) => ApiClient.UpdateAsync(id, dto);
    protected override Task<ApiResult> DeleteAsync(Guid id) => ApiClient.DeleteAsync(id);

    protected override List<CompanyResponse> Sort(List<CompanyResponse> items) => items.OrderBy(c => c.Name).ToList();

    protected override Guid GetId(CompanyResponse item) => item.Id;
    protected override CompanyFormModel ToFormModel(CompanyResponse item) => new() { Name = item.Name, Email = item.Email };
    protected override CompanyCreateRequest ToCreateDto(CompanyFormModel model) => new() { Name = model.Name, Email = model.Email };
    protected override CompanyUpdateRequest ToUpdateDto(CompanyFormModel model) => new() { Name = model.Name, Email = model.Email };

    protected override string? Validate(CompanyFormModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Name)) return "Il nome dell'azienda è obbligatorio.";
        if (string.IsNullOrWhiteSpace(model.Email)) return "L'email dell'azienda è obbligatoria.";
        return null;
    }
}

// Modello del form, esposto (non-privato) perché usato anche da CompanyFormModal.razor.
public sealed class CompanyFormModel
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
