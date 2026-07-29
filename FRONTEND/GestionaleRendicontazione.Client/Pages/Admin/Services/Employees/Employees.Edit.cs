using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

public partial class Employees
{
    private bool _formModalOpen;
    private Guid _editingId;
    private EmployeeFormModel? _formModel;

    private void OpenEditModal(EmployeeResponse employee)
    {
        _editingId = employee.Id;
        _formModel = new EmployeeFormModel
        {
            Username = employee.Username,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
        };
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
        if (string.IsNullOrWhiteSpace(_formModel.Username))
        {
            Crud.SetClientModalError("Lo username è obbligatorio.");
            return;
        }
        if (string.IsNullOrWhiteSpace(_formModel.FirstName))
        {
            Crud.SetClientModalError("Il nome è obbligatorio.");
            return;
        }
        if (string.IsNullOrWhiteSpace(_formModel.LastName))
        {
            Crud.SetClientModalError("Il cognome è obbligatorio.");
            return;
        }

        await Crud.RunCrudAsync<EmployeeResponse>(
            operation: () => EmployeeApiClient.UpdateAsync(_editingId, new EmployeeUpdateRequest
            {
                Username = _formModel.Username,
                FirstName = _formModel.FirstName,
                LastName = _formModel.LastName,
            }),
            networkErrorMessage: "Errore di rete. Riprova più tardi.",
            onSuccess: saved =>
            {
                ApplySaved(saved);
                _ = FilterState.ReloadLookupsAsync();
                CloseFormModal();
            });
    }
}

public sealed class EmployeeFormModel
{
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}