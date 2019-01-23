namespace Naos.Sample.Countries.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class CountryUpdatedDomainEventHandler
        : IDomainEventHandler<EntityUpdatedDomainEvent>
    {
        private readonly ILogger<CountryUpdatedDomainEventHandler> logger;

        public CountryUpdatedDomainEventHandler(ILogger<CountryUpdatedDomainEventHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public bool CanHandle(EntityUpdatedDomainEvent notification)
        {
            return notification?.Entity.Is<Country>() == true;
        }

        public async Task Handle(EntityUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (this.CanHandle(notification))
                {
                    this.logger.LogInformation($"{{LogKey}} handle {notification.GetType().Name.SubstringTill("DomainEvent")} (entity={notification.Entity.GetType().PrettyName()}, handler={this.GetType().PrettyName()})", LogEventKeys.DomainEvent);

                    // TODO: do something, trigger message (integration)
                }
            });
        }
    }
}