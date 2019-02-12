namespace Naos.Core.Operations.App
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

            logEvent.AddPropertyIfAbsent(new LogEventProperty("ns_ticks", new ScalarValue(DateTime.UtcNow.Ticks)));
        }
    }
}
