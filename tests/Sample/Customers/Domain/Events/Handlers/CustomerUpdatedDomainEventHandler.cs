namespace Naos.Sample.Customers.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class CustomerUpdatedDomainEventHandler
        : IDomainEventHandler<EntityUpdatedDomainEvent>
    {
        private readonly ILogger<CustomerUpdatedDomainEventHandler> logger;

        public CustomerUpdatedDomainEventHandler(ILogger<CustomerUpdatedDomainEventHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public bool CanHandle(EntityUpdatedDomainEvent notification)
        {
            return notification?.Entity.Is<Customer>() == true;
        }

        public async Task Handle(EntityUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if(this.CanHandle(notification))
                {
                    this.logger.LogInformation($"{{LogKey:l}} handle {notification.GetType().Name.SubstringTill("DomainEvent")} (entity={notification.Entity.GetType().PrettyName()}, handler={this.GetType().PrettyName()})", LogEventKeys.DomainEvent);

                    // TODO: do something, trigger message (integration)
                }
            });
        }
    }
}