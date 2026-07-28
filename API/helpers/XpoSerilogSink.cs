using System;
using DevExpress.Xpo;
using Serilog.Core;
using Serilog.Events;
using GestionaleRendicontazione.Domain.Entities;

namespace GestionaleRendicontazione.Api.Helpers
{
    /// <summary>
    /// Helper per leggere in modo tipizzato le proprietà arricchite di un <see cref="LogEvent"/>
    /// (quelle pushate da <see cref="RequestContextEnricher"/> o da un <c>LogContext.PushProperty</c>).
    /// Serilog incapsula i valori scalari in <see cref="ScalarValue"/>; qui estraiamo solo quelli
    /// convertendoli in stringa con <c>ToString()</c>, così i sink (DB, file, …) non ricevono
    /// il quoting automatico di Serilog né un null quando la proprietà manca.
    /// </summary>
    internal static class SerilogProperties
    {
        public static string? GetScalarString(LogEvent logEvent, string propertyName)
        {
            if (logEvent.Properties.TryGetValue(propertyName, out var value)
                && value is ScalarValue scalar
                && scalar.Value is not null)
            {
                return scalar.Value.ToString();
            }
            return null;
        }
    }

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
                        // Serilog emette il Timestamp come DateTimeOffset in UTC.
                        // Su SQLite (senza informazioni di Kind) il valore verrebbe
                        // riletto come ora locale e mostrerebbe uno sfasamento di +2h
                        // in estate. Convertiamo esplicitamente in ora locale italiana
                        // prima di persisterlo, così la lettura torna coerente.
                        Data = logEvent.Timestamp.LocalDateTime,
                        Livello = logEvent.Level.ToString(),
                        Messaggio = logEvent.RenderMessage(),
                        StackTrace = logEvent.Exception?.StackTrace ?? string.Empty,
                        Metodo = SerilogProperties.GetScalarString(logEvent, "RequestMethod") ?? string.Empty,
                        Path = SerilogProperties.GetScalarString(logEvent, "RequestPath") ?? string.Empty,
                        UserId = SerilogProperties.GetScalarString(logEvent, "UserId") ?? string.Empty
                    };
                    uow.CommitChanges();
                }
            }
            catch (Exception ex)
            {
                // Non richiamiamo logger.LogError qui: rientreremmo in questo stesso
                // sink e, se il DB è davvero giù, fallirebbe di nuovo all'infinito.
                // SelfLog è il canale diagnostico interno di Serilog, pensato apposta
                // per questi casi: scrive su stderr, fuori dalla pipeline dei sink.
                Serilog.Debugging.SelfLog.WriteLine("Impossibile scrivere il log su DB (XpoSerilogSink): {0}", ex);
            }
        }
    }
}