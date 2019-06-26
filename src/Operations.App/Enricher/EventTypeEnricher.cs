namespace Naos.Core.Operations.App
{
    using System;
    using System.Text;
    using global::Serilog.Core;
    using global::Serilog.Events;
    using Murmur;
    using Naos.Foundation;

    public class EventTypeEnricher : ILogEventEnricher
    {
        private static readonly Murmur32 HashAlgorithm = MurmurHash.Create32();

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            // origin: https://nblumhardt.com/2015/10/assigning-event-types-to-serilog-events/
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(
                LogEventPropertyKeys.EventType,
                BitConverter.ToUInt32(HashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(logEvent.MessageTemplate.Text)), 0)));
        }
    }
}
