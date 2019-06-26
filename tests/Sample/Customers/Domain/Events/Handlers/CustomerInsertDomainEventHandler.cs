﻿namespace Naos.Sample.Customers.Domain
{
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class CustomerInsertDomainEventHandler
        : EntityInsertDomainEventHandler
    {
        public CustomerInsertDomainEventHandler(ILogger<EntityInsertDomainEventHandler> logger)
            : base(logger)
        {
        }

        public override bool CanHandle(EntityInsertDomainEvent notification)
        {
            return notification?.Entity.Is<Customer>() == true;
        }
    }
}
