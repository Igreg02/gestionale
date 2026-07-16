using System;
using Serilog.Core;
using Serilog.Events;
using DevExpress.Xpo;
using GestionaleRendicontazione.Domain.Entities;

namespace GestionaleRendicontazione.Api.Helpers
{
    public class XpoSerilogSink : ILogEventSink
    {
        private readonly IDataLayer _dataLayer;

        public XpoSerilogSink(IDataLayer dataLayer)
        {
            _dataLayer = dataLayer;
        }

        public void Emit(LogEvent logEvent)
        {
            // Evitiamo loop infiniti ignorando i log generati da XPO stessa
            if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext) && 
                sourceContext.ToString().Contains("DevExpress.Xpo"))
            {
                return;
            }

            try
            {
                using (var uow = new UnitOfWork(_dataLayer))
                {
                    var logDb = new LogApplicativo(uow)
                    {
                        Data = logEvent.Timestamp.DateTime,
                        Livello = logEvent.Level.ToString(),
                        Messaggio = logEvent.RenderMessage(),
                        StackTrace = logEvent.Exception?.StackTrace ?? string.Empty,
                        Metodo = GetPropertyValue(logEvent, "RequestMethod"),
                        Path = GetPropertyValue(logEvent, "RequestPath"),
                        UserId = GetPropertyValue(logEvent, "UserId")
                    };
                    uow.CommitChanges();
                }
            }
            catch
            {
                // Fallback silenzioso per non far crashare l'app se il DB è giù
            }
        }

        // Estrae il valore "grezzo" di una proprietà arricchita via LogContext
        // (senza le virgolette che Serilog aggiunge di default ai ScalarValue string)
        private static string GetPropertyValue(LogEvent logEvent, string propertyName)
        {
            if (logEvent.Properties.TryGetValue(propertyName, out var value) &&
                value is ScalarValue scalarValue)
            {
                return scalarValue.Value?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }
    }
}