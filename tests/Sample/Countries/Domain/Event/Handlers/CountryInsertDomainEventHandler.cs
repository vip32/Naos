namespace Naos.Sample.Countries.Domain
{
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class CountryInsertDomainEventHandler
        : EntityInsertDomainEventHandler
    {
        public CountryInsertDomainEventHandler(ILogger<EntityInsertDomainEventHandler> logger)
            : base(logger)
        {
        }

        public override bool CanHandle(EntityInsertDomainEvent notification)
        {
            return notification?.Entity.Is<Country>() == true;
        }
    }
}
