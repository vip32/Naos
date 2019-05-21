namespace Naos.Core.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Naos.Core.Common;

    public class DomainEvents
    {
        /// <summary>
        /// The domain registered events.
        /// </summary>
        private readonly ICollection<IDomainEvent> registrations = new List<IDomainEvent>(); // TODO: concurrent collection?

        /// <summary>
        /// Gets all registered domain events.
        /// </summary>
        public IEnumerable<IDomainEvent> GetAll() => this.registrations;

        /// <summary>
        /// Registers the domain event to publish.
        /// Domain Events are only registered on the aggregate root because it is ensuring the integrity of the aggregate as a whole.
        /// </summary>
        /// <param name="event">The event.</param>
        public void Register(IDomainEvent @event)
        {
            EnsureArg.IsNotNull(@event, nameof(@event));

            this.registrations.Add(@event);
        }

        public async Task DispatchAsync(IMediator mediator)
        {
            EnsureArg.IsNotNull(mediator, nameof(mediator));

            foreach(var @event in this.GetAll())
            {
                await mediator.Publish(@event).AnyContext();
            }

            this.Clear();
        }

        /// <summary>
        /// Clears the registered domain events.
        /// </summary>
        public void Clear()
        {
            this.registrations.Clear();
        }
    }
}
