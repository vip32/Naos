namespace Naos.Sample.Countries.Domain
{
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class CountryUpdateDomainEventHandler
        : EntityUpdateDomainEventHandler
    {
        public CountryUpdateDomainEventHandler(ILoggerFactory loggerFactory)
           : base(loggerFactory)
        {
        }

        public override bool CanHandle(EntityUpdateDomainEvent notification)
        {
            return notification?.Entity.Is<Country>() == true;
        }
    }
}