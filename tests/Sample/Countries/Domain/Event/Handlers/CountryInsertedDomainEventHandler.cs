namespace Naos.Sample.Countries.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class CountryInsertedDomainEventHandler
        : IDomainEventHandler<EntityInsertedDomainEvent>
    {
        private readonly ILogger<CountryInsertedDomainEventHandler> logger;

        public CountryInsertedDomainEventHandler(ILogger<CountryInsertedDomainEventHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public bool CanHandle(EntityInsertedDomainEvent notification)
        {
            return notification?.Entity.Is<Country>() == true;
        }

        public async Task Handle(EntityInsertedDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (this.CanHandle(notification))
                {
                    this.logger.LogInformation($"{{LogKey:l}} handle {notification.GetType().Name.SubstringTill("DomainEvent")} (entity={notification.Entity.GetType().PrettyName()}, handler={this.GetType().PrettyName()})", LogEventKeys.DomainEvent);

                    // TODO: do something, trigger message (integration)
                }
            });
        }
    }
}
