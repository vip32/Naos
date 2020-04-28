namespace Naos.Sample.Countries.Domain
{
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class CountryInsertDomainEventHandler
        : EntityInsertDomainEventHandler
    {
        protected CountryInsertDomainEventHandler(ILoggerFactory loggerFactory)
           : base(loggerFactory)
        {
        }

        public override bool CanHandle(EntityInsertDomainEvent notification)
        {
            return notification?.Entity.Is<Country>() == true;
        }
    }
}
