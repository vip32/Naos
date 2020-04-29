namespace EventSourcingCQRS.Domain.EventStore
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using EnsureThat;
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
            EnsureArg.IsNotNull(connection, nameof(connection));

            this.connection = connection;
        }

        public async Task<AppendResult> AppendEventAsync<TId>(IDomainEvent<TId> @event)
        //where TId : IAggregateId
        {
            EnsureArg.IsNotNull(@event, nameof(@event));

            try
            {
                var eventData = new EventData(
                    @event.EventId,
                    @event.GetType().AssemblyQualifiedName,
                    true,
                    this.Serialize(@event),
                    Encoding.UTF8.GetBytes("{}")); // TODO: CorrelationId as metadata?

                var writeResult = await this.connection.AppendToStreamAsync(
                    @event.AggregateId.ToString(),
                    @event.AggregateVersion == EventSourcedAggregateRoot<TId>.NewVersion ? ExpectedVersion.NoStream : @event.AggregateVersion,
                    eventData).AnyContext();

                return new AppendResult(writeResult.NextExpectedVersion);
            }
            catch (EventStoreConnectionException ex)
            {
                throw new EventStoreCommunicationException($"error while appending event {@event.EventId} for aggregate {@event.AggregateId}", ex);
            }
        }

        public async Task<IEnumerable<Event<TId>>> ReadEventsAsync<TId>(TId id, long? fromVersion = null, long? toVersion = null)
            //where TAggregateId : IAggregateId
        {
            try
            {
                var result = new List<Event<TId>>();
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
                        result.Add(new Event<TId>(this.Deserialize<TId>(resolvedEvent.Event.EventType, resolvedEvent.Event.Data), resolvedEvent.Event.EventNumber));
                        // TODO: yield return?
                    }
                }
                while (!currentSlice.IsEndOfStream);

                return result;
            }
            catch (EventStoreConnectionException ex)
            {
                throw new EventStoreCommunicationException($"error while reading events for aggregate {id}", ex);
            }
        }

        private IDomainEvent<TId> Deserialize<TId>(string eventType, byte[] data)
        {
            // TODO: replace with ISerializer
            var settings = new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver() };
            return (IDomainEvent<TId>)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType(eventType), settings);
        }

        private byte[] Serialize<TId>(IDomainEvent<TId> @event)
        {
            // TODO: replace with ISerializer
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
        }
    }
}
