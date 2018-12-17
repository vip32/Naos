namespace Naos.Sample.UserAccounts.Domain
{
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class UserAccountInsertDomainEventHandler
        : EntityInsertDomainEventHandler
    {
        public UserAccountInsertDomainEventHandler(ILogger<EntityInsertDomainEventHandler> logger)
            : base(logger)
        {
        }

        public override bool CanHandle(EntityInsertDomainEvent notification)
        {
            return notification?.Entity.Is<UserAccount>() == true;
        }
    }
}
