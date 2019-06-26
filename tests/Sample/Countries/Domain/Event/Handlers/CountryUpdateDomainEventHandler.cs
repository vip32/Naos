namespace Naos.Sample.Countries.Domain
{
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class CountryUpdateDomainEventHandler
        : EntityUpdateDomainEventHandler
    {
        public CountryUpdateDomainEventHandler(ILogger<EntityUpdateDomainEventHandler> logger)
            : base(logger)
        {
        }

        public override bool CanHandle(EntityUpdateDomainEvent notification)
        {
            return notification?.Entity.Is<Country>() == true;
        }
    }
}