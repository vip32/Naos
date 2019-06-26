namespace Naos.Sample.UserAccounts.Domain
{
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    public class UserAccountUpdateDomainEventHandler
        : EntityUpdateDomainEventHandler
    {
        public UserAccountUpdateDomainEventHandler(ILogger<EntityUpdateDomainEventHandler> logger)
            : base(logger)
        {
        }

        public override bool CanHandle(EntityUpdateDomainEvent notification)
        {
            return notification?.Entity.Is<UserAccount>() == true;
        }
    }
}