// Helper JS invocato da C# via IJSRuntime per scaricare un Blob costruito lato client.
// Usato dal PdfReportService per offrire il file PDF generato all'utente.
//
// Esempio d'uso da C#:
//   await JS.InvokeVoidAsync("downloadHelper.saveAs", fileName, "application/pdf", base64String);
//
// Implementazione: ricostruiamo il Blob da base64 (più semplice da passare
// attraverso JSInterop rispetto a un ArrayBuffer, soprattutto per array grandi)
// e poi clicchiamo programmaticamente un <a download> con URL.createObjectURL.
window.downloadHelper = {
  saveAs: function (fileName, contentType, base64Data) {
    try {
      const binary = atob(base64Data);
      const len = binary.length;
      const bytes = new Uint8Array(len);
      for (let i = 0; i < len; i++) bytes[i] = binary.charCodeAt(i);
      const blob = new Blob([bytes], { type: contentType || "application/octet-stream" });

      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = fileName;
      a.style.display = "none";
      document.body.appendChild(a);
      a.click();
      // Cleanup: rimuoviamo l'anchor e liberiamo l'object URL dopo un attimo,
      // così il browser ha il tempo di iniziare il download.
      setTimeout(function () {
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
      }, 1000);

      return true;
    } catch (err) {
      console.error("downloadHelper.saveAs error:", err);
      return false;
    }
  }
};
