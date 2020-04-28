namespace EventSourcingCQRS.Domain.EventStore
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using global::EventStore.ClientAPI;
    using global::EventStore.ClientAPI.Exceptions;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Domain.EventSourcing;
    using Newtonsoft.Json;

    public class EventStoreEventStore : IEventStore
    {
        private readonly IEventStoreConnection connection;

        public EventStoreEventStore(IEventStoreConnection connection)
        {
            this.connection = connection;
        }

        public async Task<IEnumerable<Event<TId>>> ReadEventsAsync<TId>(TId id)
            //where TAggregateId : IAggregateId
        {
            try
            {
                var ret = new List<Event<TId>>();
                StreamEventsSlice currentSlice;
                long nextSliceStart = StreamPosition.Start;

                do
                {
                    currentSlice = await this.connection.ReadStreamEventsForwardAsync(id.ToString(), nextSliceStart, 200, false).AnyContext();
                    if (currentSlice.Status != SliceReadStatus.Success)
                    {
                        throw new EventStoreAggregateNotFoundException($"aggregate {id} not found");
                    }

                    nextSliceStart = currentSlice.NextEventNumber;
                    foreach (var resolvedEvent in currentSlice.Events)
                    {
                        ret.Add(new Event<TId>(this.Deserialize<TId>(resolvedEvent.Event.EventType, resolvedEvent.Event.Data), resolvedEvent.Event.EventNumber));
                    }
                }
                while (!currentSlice.IsEndOfStream);

                return ret;
            }
            catch (EventStoreConnectionException ex)
            {
                throw new EventStoreCommunicationException($"error while reading events for aggregate {id}", ex);
            }
        }

        public async Task<AppendResult> AppendEventAsync<TId>(IDomainEvent<TId> @event)
            //where TId : IAggregateId
        {
            try
            {
                var eventData = new EventData(
                    @event.EventId,
                    @event.GetType().AssemblyQualifiedName,
                    true,
                    this.Serialize(@event),
                    Encoding.UTF8.GetBytes("{}")); // TODO: CorrelationId as metadata

                var writeResult = await this.connection.AppendToStreamAsync(
                    @event.AggregateId.ToString(),
                    @event.AggregateVersion == EventSourcingAggregateRoot<TId>.NewAggregateVersion ? ExpectedVersion.NoStream : @event.AggregateVersion,
                    eventData).AnyContext();

                return new AppendResult(writeResult.NextExpectedVersion);
            }
            catch (EventStoreConnectionException ex)
            {
                throw new EventStoreCommunicationException($"error while appending event {@event.EventId} for aggregate {@event.AggregateId}", ex);
            }
        }

        private IDomainEvent<TAggregateId> Deserialize<TAggregateId>(string eventType, byte[] data)
        {
            var settings = new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver() };
            return (IDomainEvent<TAggregateId>)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType(eventType), settings);
        }

        private byte[] Serialize<TAggregateId>(IDomainEvent<TAggregateId> @event)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
        }
    }
}
