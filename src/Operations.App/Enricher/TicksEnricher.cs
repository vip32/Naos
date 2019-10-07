namespace Naos.Operations.App
{
    using System;
    using global::Serilog.Core;
    using global::Serilog.Events;
    using Microsoft.Extensions.Logging;

    public class TicksEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            if (logEvent == null)
            {
                return;
            }

            logEvent.AddPropertyIfAbsent(
                new LogEventProperty(LogPropertyKeys.Ticks, new ScalarValue(DateTime.UtcNow.Ticks)));
        }
    }
}
