﻿namespace Naos.Sample.Inventory.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class InventoryInsertedDomainEventHandler
        : IDomainEventHandler<EntityInsertedDomainEvent>
    {
        private readonly ILogger<InventoryInsertedDomainEventHandler> logger;

        public InventoryInsertedDomainEventHandler(ILogger<InventoryInsertedDomainEventHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public bool CanHandle(EntityInsertedDomainEvent notification)
        {
            return notification?.Entity.Is<ProductInventory>() == true;
        }

        public async Task Handle(EntityInsertedDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                if (this.CanHandle(notification))
                {
                    this.logger.LogInformation($"{{LogKey:l}} handle {notification.GetType().Name.SliceTill("DomainEvent")} (entity={notification.Entity.GetType().PrettyName()}, handler={this.GetType().PrettyName()})", LogKeys.DomainEvent);

                    // TODO: do something, trigger message (integration)
                }
            }).AnyContext();
        }
    }
}
