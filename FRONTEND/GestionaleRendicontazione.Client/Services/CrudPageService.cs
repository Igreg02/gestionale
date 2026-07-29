using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GestionaleRendicontazione.Client.Services
{
    public sealed class CrudPageService
    {

        public bool IsLoading { get; private set; }
        public string? ErrorMessage { get; private set; }
        public bool IsSaving { get; private set; }
        public string? ModalError { get; private set; }

        public event Action? OnChanged;

        public void ResetModalError() => SetModalError(null);

        public void SetClientModalError(string message) => SetModalError(message);

        public void ResetModalAndPageErrors()
        {
            SetModalError(null);
            if (ErrorMessage is not null) ErrorMessage = null;
            NotifyChanged();
        }

        public void SetErrorMessage(string? message)
        {
            ErrorMessage = message;
            NotifyChanged();
        }

        public void NotifyStateChanged() => NotifyChanged();

        private void SetModalError(string? message)
        {
            ModalError = message;
            NotifyChanged();
        }


        public static string FormatError(
            Dictionary<string, string[]> validationErrors,
            string? errorMessage,
            int statusCode = 0)
        {
            if (statusCode == 400)
                return ApiResultExtensions.ToUserMessage(statusCode, validationErrors, errorMessage, null);

            if (validationErrors.Count > 0)
                return string.Join(" ", validationErrors.SelectMany(kv => kv.Value));

            return ApiResultExtensions.ToUserMessage(statusCode, validationErrors, errorMessage, null);
        }

        public async Task RunLoadAsync(Func<Task> load, string errorMessage = "Impossibile recuperare i dati dal server. Riprova più tardi.")
        {
            IsLoading = true;
            ErrorMessage = null;
            NotifyChanged();

            try
            {
                await load();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore nel caricamento: {ex}");
                ErrorMessage = errorMessage;
            }
            finally
            {
                IsLoading = false;
                NotifyChanged();
            }
        }

        public async Task<bool> RunCrudAsync(
            Func<Task<ApiResult>> operation,
            string networkErrorMessage,
            Action? onSuccess = null)
        {
            IsSaving = true;
            ModalError = null;
            NotifyChanged();

            try
            {
                var result = await operation();
                if (!result.IsSuccess)
                {
                    ModalError = FormatError(result.ValidationErrors, result.ErrorMessage, result.StatusCode);
                    return false;
                }

                onSuccess?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore CRUD: {ex}");
                ModalError = networkErrorMessage;
                return false;
            }
            finally
            {
                IsSaving = false;
                NotifyChanged();
            }
        }

        public async Task<ApiResult<T>?> RunCrudAsync<T>(
            Func<Task<ApiResult<T>>> operation,
            string networkErrorMessage,
            Action<T>? onSuccess = null)
        {
            IsSaving = true;
            ModalError = null;
            NotifyChanged();

            try
            {
                var result = await operation();
                if (!result.IsSuccess)
                {
                    ModalError = FormatError(result.ValidationErrors, result.ErrorMessage, result.StatusCode);
                    return result;
                }

                if (result.Data is not null)
                    onSuccess?.Invoke(result.Data);
                return result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Errore CRUD: {ex}");
                ModalError = networkErrorMessage;
                return null;
            }
            finally
            {
                IsSaving = false;
                NotifyChanged();
            }
        }

        private void NotifyChanged() => OnChanged?.Invoke();
    }
}
