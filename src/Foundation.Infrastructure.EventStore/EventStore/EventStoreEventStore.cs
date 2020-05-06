namespace Naos.Foundation.Infrastructure.EventSourcing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        private readonly JsonSerializerSettings serializerSetings = new JsonSerializerSettings { ContractResolver = new PrivateSetterContractResolver() };

        public EventStoreEventStore(IEventStoreConnection connection)
        {
            EnsureArg.IsNotNull(connection, nameof(connection));

            this.connection = connection;
        }

        public async Task<IEnumerable<Event<TId>>> ReadEventsAsync<TId>(string streamName, long? fromVersion = null, long? toVersion = null)
        {
            EnsureArg.IsNotNullOrEmpty(streamName, nameof(streamName));

            try
            {
                var result = new List<Event<TId>>();
                StreamEventsSlice currentSlice;
                var nextSliceStart = fromVersion ??= StreamPosition.Start;
                // TODO: incorporate toVersion

                do
                {
                    currentSlice = await this.connection.ReadStreamEventsForwardAsync(streamName, nextSliceStart, 200, false).AnyContext();
                    if (currentSlice.Status != SliceReadStatus.Success)
                    {
                        throw new EventStoreStreamNotFoundException($"event stream {streamName} not found");
                    }

                    nextSliceStart = currentSlice.NextEventNumber;
                    foreach (var resolvedEvent in currentSlice.Events)
                    {
                        result.Add(new Event<TId>(
                            this.Deserialize<TId>(resolvedEvent.Event.EventType, resolvedEvent.Event.Data), resolvedEvent.Event.EventNumber));
                        // TODO: yield return?
                    }
                }
                while (!currentSlice.IsEndOfStream);

                return result;
            }
            catch (EventStoreConnectionException ex)
            {
                throw new EventStoreCommunicationException($"error while reading events for stream {streamName}", ex);
            }
        }

        public async Task<EventResult> SaveEventAsync<TId>(string streamName, IDomainEvent<TId> @event)
        //where TId : IAggregateId
        {
            EnsureArg.IsNotNullOrEmpty(streamName, nameof(streamName));
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
                    streamName,
                    @event.AggregateVersion == EventSourcedAggregateRoot<TId>.NewVersion ? ExpectedVersion.NoStream : @event.AggregateVersion,
                    eventData).AnyContext();
                // https://eventstore.com/docs/dotnet-api/optimistic-concurrency-and-idempotence/index.html
                return new EventResult(writeResult.NextExpectedVersion);
            }
            catch (EventStoreConnectionException ex)
            {
                throw new EventStoreCommunicationException($"error while storing event {@event.EventId} for aggregate {@event.AggregateId}", ex);
            }
        }

        public async Task<Snapshot<TAggregate, TId>> ReadSnapshotAsync<TAggregate, TId>(string streamName) // latest snapshot
            where TAggregate : EventSourcedAggregateRoot<TId>, IEventSourcedAggregateRoot<TId>
        {
            EnsureArg.IsNotNullOrEmpty(streamName, nameof(streamName));

            streamName += "-snapshots";
            var events = await this.connection.ReadStreamEventsBackwardAsync(streamName, StreamPosition.End, 1, false).AnyContext();
            if (events?.Events?.Any() == true)
            {
                return new Snapshot<TAggregate, TId>(
                    this.Deserialize<TAggregate, TId>(events.Events[0].Event.EventType, events.Events[0].Event.Data), events.Events[0].Event.EventNumber);
            }

            throw new EventStoreStreamNotFoundException($"snapshot stream {streamName} not found");
        }

        public async Task<EventResult> SaveSnapshotAsync<TAggregate, TId>(string streamName, TAggregate aggregate)
            where TAggregate : EventSourcedAggregateRoot<TId>, IEventSourcedAggregateRoot<TId>
        {
            EnsureArg.IsNotNullOrEmpty(streamName, nameof(streamName));

            streamName += "-snapshots";
            var eventId = Guid.NewGuid();
            try
            {
                var eventData = new EventData(
                    eventId,
                    aggregate.GetType().AssemblyQualifiedName,
                    true,
                    this.Serialize<TAggregate, TId>(aggregate),
                    Encoding.UTF8.GetBytes("{}")); // TODO: CorrelationId as metadata?

                var writeResult = await this.connection.AppendToStreamAsync(streamName, ExpectedVersion.Any, eventData).AnyContext();
                return new EventResult(writeResult.NextExpectedVersion);
            }
            catch (EventStoreConnectionException ex)
            {
                throw new EventStoreCommunicationException($"error while storing snapshot {eventId} for aggregate {aggregate.Id}", ex);
            }
        }

        private IDomainEvent<TId> Deserialize<TId>(string eventType, byte[] data)
        {
            // TODO: replace with ISerializer
            return (IDomainEvent<TId>)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType(eventType), this.serializerSetings);
        }

        private TAggregate Deserialize<TAggregate, TId>(string aggregateType, byte[] data)
            where TAggregate : EventSourcedAggregateRoot<TId>, IEventSourcedAggregateRoot<TId>
        {
            // TODO: replace with ISerializer
            return (TAggregate)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), Type.GetType(aggregateType), this.serializerSetings);
        }

        private byte[] Serialize<TId>(IDomainEvent<TId> @event)
        {
            // TODO: replace with ISerializer
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
        }

        private byte[] Serialize<TAggregate, TId>(TAggregate aggregate)
            where TAggregate : EventSourcedAggregateRoot<TId>, IEventSourcedAggregateRoot<TId>
        {
            // TODO: replace with ISerializer
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(aggregate));
        }
    }
}
