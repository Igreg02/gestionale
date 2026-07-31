using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace GestionaleRendicontazione.Client.Services.Reports
{
    /// <summary>
    /// Wrapper sottile sopra IJSRuntime per scaricare file generati lato client
    /// (es. PDF di PdfReportService). Mantiene qui la complessità della conversione
    /// byte[] → base64, così le pagine Blazor possono chiamare un solo metodo.
    /// </summary>
    public sealed class DownloadInterop
    {
        private readonly IJSRuntime _js;

        public DownloadInterop(IJSRuntime js)
        {
            _js = js;
        }

        /// <summary>
        /// Chiede al browser di scaricare <paramref name="data"/> come file
        /// <paramref name="fileName"/> con il content type indicato.
        /// Restituisce true se il download è stato avviato, false altrimenti.
        /// </summary>
        public async Task<bool> SaveAsAsync(string fileName, string contentType, byte[] data)
        {
            if (data is null || data.Length == 0) return false;
            var base64 = Convert.ToBase64String(data);
            var result = await _js.InvokeAsync<bool>("downloadHelper.saveAs", SanitizeFileName(fileName), contentType, base64);
            return result;
        }

        // Il nome file arriva da un campo di testo libero (es. ReportDownloadModal): rimuoviamo
        // separatori di path e caratteri non validi su Windows/macOS/Linux prima di passarlo
        // all'attributo <a download>, così l'utente non può iniettare percorsi o nomi anomali.
        private static readonly char[] InvalidFileNameChars = "\\/:*?\"<>|".ToCharArray();

        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return "download";

            var cleaned = new string(fileName
                .Where(c => !char.IsControl(c) && !InvalidFileNameChars.Contains(c))
                .ToArray())
                .Trim(' ', '.');

            if (cleaned.Length > 200)
            {
                cleaned = cleaned[..200];
            }

            return string.IsNullOrEmpty(cleaned) ? "download" : cleaned;
        }
    }
}
