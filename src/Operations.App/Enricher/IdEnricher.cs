namespace Naos.Core.Operations.App
{
    using global::Serilog.Core;
    using global::Serilog.Events;
    using Naos.Core.Common;

    public class IdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty(LogEventPropertyKeys.Id, IdGenerator.Instance.Next));
        }
    }
}
