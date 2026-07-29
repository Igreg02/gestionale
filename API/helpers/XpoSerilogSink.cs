using System;
using DevExpress.Xpo;
using Serilog.Core;
using Serilog.Events;
using GestionaleRendicontazione.Domain.Entities;

namespace GestionaleRendicontazione.Api.Helpers
{
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
                Serilog.Debugging.SelfLog.WriteLine("Impossibile scrivere il log su DB (XpoSerilogSink): {0}", ex);
            }
        }
    }
}