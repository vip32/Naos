namespace Naos.Foundation.Domain
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Naos.Foundation.Domain.EventSourcing;

    public class EventSourcingRepository<TAggregate, TId> : IEventSourcingRepository<TAggregate, TId>
        where TAggregate : EventSourcedAggregateRoot<TId>, IEventSourcedAggregateRoot<TId>
        //where TId : IAggregateId
    {
        private readonly IEventStore eventStore;
        private readonly IMediator mediator;

        public EventSourcingRepository(IEventStore eventStore, IMediator mediator)
        {
            EnsureArg.IsNotNull(eventStore, nameof(eventStore));
            EnsureArg.IsNotNull(mediator, nameof(mediator));

            this.eventStore = eventStore;
            this.mediator = mediator;
        }

        public async Task<TAggregate> GetByIdAsync(TId aggregateId)
        {
            try
            {
                var aggregate = Factory<TAggregate>.Create();
                foreach (var @event in await this.eventStore.ReadEventsAsync<TId>(this.GetStreamName(aggregateId)).AnyContext())
                {
                    aggregate.ApplyEvent(@event.DomainEvent, @event.EventNumber);
                }

                return aggregate;
            }
            catch (EventStoreAggregateNotFoundException)
            {
                return null;
            }
            catch (EventStoreCommunicationException ex)
            {
                throw new EventSourcingRepositoryException("unable to access persistence layer", ex);
            }
        }

        public async Task SaveAsync(TAggregate aggregate)
        {
            EnsureArg.IsNotNull(aggregate, nameof(aggregate));

            try
            {
                //IEventSourcingAggregate<TId> aggregatePersistence = aggregate;

                foreach (var @event in aggregate/*Persistence*/.GetChanges())
                {
                    await this.eventStore.AppendEventAsync(this.GetStreamName(aggregate.Id), @event).AnyContext();
                    await this.mediator.Publish(/*(dynamic)*/@event, CancellationToken.None).AnyContext();
                }

                aggregate/*Persistence*/.ClearChanges();
            }
            catch (EventStoreCommunicationException ex)
            {
                throw new EventSourcingRepositoryException("unable to access persistence layer", ex);
            }
        }

        private string GetStreamName(TId aggregateId)
        {
            return $"{typeof(TAggregate).Name}-{aggregateId}";
        }
    }
}
