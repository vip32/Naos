namespace Naos.Core.App.Operations.Serilog
{
    using System;
    using global::Serilog.Core;
    using global::Serilog.Events;

    public class TicksEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null)
            {
                return;
            }

            logEvent.AddPropertyIfAbsent(new LogEventProperty("Ticks", new ScalarValue(DateTime.UtcNow.Ticks)));
        }
    }
}
