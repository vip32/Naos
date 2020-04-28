namespace Naos.Sample.UserAccounts.Domain
{
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class UserAccountUpdateDomainEventHandler
        : EntityUpdateDomainEventHandler
    {
        protected UserAccountUpdateDomainEventHandler(ILoggerFactory loggerFactory)
           : base(loggerFactory)
        {
        }

        public override bool CanHandle(EntityUpdateDomainEvent notification)
        {
            return notification?.Entity.Is<UserAccount>() == true;
        }
    }
}