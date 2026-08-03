using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GestionaleRendicontazione.Client.Services
{
    /// <summary>
    /// Stato UI condiviso e orchestrazione CRUD per le pagine admin
    /// (Statuses / Types / Companies / Projects).
    ///
    /// Centralizza:
    ///  - <see cref="IsLoading"/> + <see cref="ErrorMessage"/> per il caricamento iniziale
    ///  - <see cref="IsSaving"/> + <see cref="ModalError"/> per Create/Update/Delete
    ///  - try/catch con messaggio di errore di rete italiano
    ///  - mapping degli errori HTTP a stringhe user-facing via
    ///    <see cref="ApiResultExtensions.ToUserMessage"/>
    ///
    /// Le pagine si sottoscrivono a <see cref="OnChanged"/> in OnInitializedAsync
    /// e chiamano StateHasChanged(); i campi dei partial class restano nelle pagine
    /// (lista items, _formModel, _editingId, ...) — il servizio gestisce solo
    /// lo stato UI trasversale.
    /// </summary>
    public sealed class CrudPageService
    {
        // ── Stato UI (osservabile) ────────────────────────────────────────────

        public bool IsLoading { get; private set; }
        public string? ErrorMessage { get; private set; }
        public bool IsSaving { get; private set; }
        public string? ModalError { get; private set; }

        /// <summary>
        /// Scatta quando uno qualsiasi dei 4 campi qui sopra cambia. Le pagine
        /// si sottoscrivono in OnInitializedAsync per ri-renderizzare.
        /// </summary>
        public event Action? OnChanged;

        // ── Helpers di stato ──────────────────────────────────────────────────

        /// <summary>Resetta solo <see cref="ModalError"/> (chiamato da Open*Modal).</summary>
        public void ResetModalError() => SetModalError(null);

        /// <summary>
        /// Imposta direttamente un errore di validazione client-side
        /// (es. campo obbligatorio mancante) senza passare dal flusso CRUD.
        /// </summary>
        public void SetClientModalError(string message) => SetModalError(message);

        /// <summary>Resetta sia ModalError sia (opzionalmente) ErrorMessage.</summary>
        public void ResetModalAndPageErrors()
        {
            SetModalError(null);
            if (ErrorMessage is not null) ErrorMessage = null;
            NotifyChanged();
        }

        /// <summary>
        /// Imposta direttamente <see cref="ErrorMessage"/> (utile per flussi di caricamento
        /// con logica custom, es. cancellation token + filtro che non rientrano in
        /// <see cref="RunLoadAsync"/>). Notifica i subscriber.
        /// </summary>
        public void SetErrorMessage(string? message)
        {
            ErrorMessage = message;
            NotifyChanged();
        }

        /// <summary>
        /// Notifica manualmente i subscriber (per casi in cui lo stato cambia da
        /// fonti esterne ai flussi CRUD/Load orchestrati dal servizio).
        /// </summary>
        public void NotifyStateChanged() => NotifyChanged();

        private void SetModalError(string? message)
        {
            ModalError = message;
            NotifyChanged();
        }

        // ── FormatError (sostituisce l'helper statico duplicato in 5 file) ─────

        /// <summary>
        /// Restituisce il testo italiano user-facing per un fallimento API:
        /// preferisce i messaggi di validation (400) del backend, poi il mapping
        /// per status code (409/422/5xx/...), poi il fallback generico.
        /// Thin wrapper su <see cref="ApiResultExtensions.ToUserMessage"/> — unica sede della logica.
        /// </summary>
        public static string FormatError(
            Dictionary<string, string[]> validationErrors,
            string? errorMessage,
            int statusCode = 0)
            => ApiResultExtensions.ToUserMessage(statusCode, validationErrors, errorMessage, null);

        // ── Lifecycle / CRUD orchestrators ────────────────────────────────────

        /// <summary>
        /// Avvolge il caricamento iniziale della pagina con toggle
        /// IsLoading + cattura eccezioni di rete in ErrorMessage.
        /// </summary>
        /// <param name="load">Operazione di caricamento (async).</param>
        /// <param name="errorMessage">Testo italiano mostrato in pagina se il caricamento fallisce.</param>
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

        /// <summary>
        /// Esegue un'operazione CRUD (tipicamente Delete) con toggle IsSaving,
        /// try/catch e mapping errori in ModalError. Su successo chiama onSuccess.
        /// </summary>
        /// <returns>true se l'operazione è andata a buon fine.</returns>
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

        /// <summary>
        /// Overload tipizzato per Create/Update. Su successo:
        ///  - imposta ModalError = null
        ///  - invoca onSuccess(value)
        /// Su failure imposta ModalError; l'eccezione di rete è "tradotta" in ModalError.
        /// </summary>
        /// <returns>
        /// ApiResult in caso di fallimento (con StatusCode/ValidationErrors per debug)
        /// oppure null in caso di eccezione di rete (perché lo stato è già in ModalError).
        /// </returns>
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
