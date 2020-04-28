namespace Naos.Foundation.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    public abstract class EntityInsertDomainEventHandler
        : DomainEventHandlerBase<EntityInsertDomainEvent>
    {
        protected EntityInsertDomainEventHandler(ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
        }

        public override async Task Process(EntityInsertDomainEvent notification, CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                this.Logger.LogInformation($"{{LogKey:l}} [{notification.EventId}] handle {notification.GetType().Name.SliceTill("DomainEvent")} (entity={notification.Entity.GetType().PrettyName()}, handler={this.GetType().PrettyName()})", LogKeys.DomainEvent);

                if (notification?.Entity.Is<IStateEntity>() == true) // use pattern matching
                {
                    var entity = notification.Entity.As<IStateEntity>();
                    entity.State?.SetCreated("[IDENTITY]", "domainevent"); // TODO: use current identity
                }

                if (notification?.Entity.Is<IIdentifiable>() == true) // use pattern matching
                {
                    var entity = notification.Entity.As<IIdentifiable>();
                    entity.SetIdentifierHash();
                }
            }).AnyContext();
        }
    }
}
