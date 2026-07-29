using System;

namespace GestionaleRendicontazione.Domain.Dtos
{
    /// <summary>
    /// DTO per i log applicativi. Solo lettura: la pagina admin di Logs non
    /// consente creazione/modifica/eliminazione (i log sono alimentati dal
    /// sink Serilog). I campi "StackTrace", "OptimisticLockField" e "GCRecord"
    /// del'entità <c>LogApplicativo</c> sono esclusi volutamente dal payload.
    /// </summary>
    public class LogDto
    {
        public class Response
        {
            public int Id { get; set; }
            public DateTime Data { get; set; }
            public string Livello { get; set; } = string.Empty;
            public string Messaggio { get; set; } = string.Empty;
            public string Metodo { get; set; } = string.Empty;
            public string Path { get; set; } = string.Empty;
            public string UserId { get; set; } = string.Empty;
        }
    }
}
