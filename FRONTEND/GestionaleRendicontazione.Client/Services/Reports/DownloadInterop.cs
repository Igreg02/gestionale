using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace GestionaleRendicontazione.Client.Services.Reports
{
    public sealed class DownloadInterop
    {
        private readonly IJSRuntime _js;

        public DownloadInterop(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<bool> SaveAsAsync(string fileName, string contentType, byte[] data)
        {
            if (data is null || data.Length == 0) return false;
            var base64 = Convert.ToBase64String(data);
            var result = await _js.InvokeAsync<bool>("downloadHelper.saveAs", fileName, contentType, base64);
            return result;
        }
    }
}
