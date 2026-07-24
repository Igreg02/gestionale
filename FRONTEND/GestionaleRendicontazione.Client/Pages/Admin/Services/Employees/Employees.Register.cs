using GestionaleRendicontazione.Client.Services;

namespace GestionaleRendicontazione.Client.Pages.Admin;

// Logica della modale "Nuovo dipendente" (crea nuovo Employee via POST /api/auth/register).
public partial class Employees
{
    private bool _registerModalOpen;
    private RegisterFormModel? _registerModel;
    private bool _isRegistering;
    private string? _registerError;

    private void OpenRegisterModal()
    {
        _registerModel = new RegisterFormModel();
        _registerError = null;
        _registerModalOpen = true;
    }

    private void CloseRegisterModal()
    {
        _registerModalOpen = false;
        _registerModel = null;
        _registerError = null;
    }

    private async Task SaveRegisterAsync()
    {
        if (_registerModel is null) return;

        if (string.IsNullOrWhiteSpace(_registerModel.UserName)) { _registerError = "Lo username è obbligatorio."; return; }
        if (string.IsNullOrWhiteSpace(_registerModel.Password) || _registerModel.Password.Length < 6) { _registerError = "La password deve essere di almeno 6 caratteri."; return; }
        if (string.IsNullOrWhiteSpace(_registerModel.FirstName)) { _registerError = "Il nome è obbligatorio."; return; }
        if (string.IsNullOrWhiteSpace(_registerModel.LastName)) { _registerError = "Il cognome è obbligatorio."; return; }

        _isRegistering = true;
        _registerError = null;

        try
        {
            var result = await AuthService.RegisterAsync(new RegisterRequestDto
            {
                UserName = _registerModel.UserName,
                Password = _registerModel.Password,
                FirstName = _registerModel.FirstName,
                LastName = _registerModel.LastName,
            });

            if (!result.IsSuccess)
            {
                _registerError = FormatError(result.ValidationErrors, result.ErrorMessage, result.StatusCode);
                return;
            }

            var created = result.Data!;
            _employees.Add(new EmployeeResponse
            {
                Id = created.Id,
                Username = created.UserName,
                FirstName = created.FirstName,
                LastName = created.LastName,
            });
            _employees = _employees.OrderBy(e => e.Username).ToList();
            _ = FilterState.ReloadLookupsAsync();

            CloseRegisterModal();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Errore nella registrazione del dipendente: {ex}");
            _registerError = "Errore di rete. Riprova più tardi.";
        }
        finally
        {
            _isRegistering = false;
        }
    }
}

// Modello del form, esposto (non-privato) perché usato anche da EmployeeRegisterModal.razor.
public sealed class RegisterFormModel
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
