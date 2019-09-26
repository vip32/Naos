namespace Naos.Operations.App
{
    using global::Serilog.Core;
    using global::Serilog.Events;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    public class IdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty(LogPropertyKeys.Id, IdGenerator.Instance.Next));
        }
    }
}
