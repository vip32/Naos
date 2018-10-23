namespace Naos.Core.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Common;

    public class EntityCreateDomainEventDomainEventHandler : IDomainEventHandler<EntityCreateDomainEvent<IEntity>>
    {
        public Task Handle(EntityCreateDomainEvent<IEntity> notification, CancellationToken cancellationToken)
        {
            if (notification.Is<IStateEntity>())
            {
                notification.As<IStateEntity>().State.CreatedDate = new DateTimeEpoch();
            }

            if (notification.Is<IVersionIdentified>())
            {
                notification.As<IVersionIdentified>().UpdateVersionIdentifier();
            }

            return null;
        }
    }

    //#pragma warning disable SA1402 // File may only contain a single class
    //#pragma warning disable SA1649 // File name must match first type name
    //    public class EntityCreateDomainEventDomainEvent2Handler : MediatR.INotificationHandler<EntityCreateDomainEvent2<IEntity>>
    //#pragma warning restore SA1649 // File name must match first type name
    //#pragma warning restore SA1402 // File may only contain a single class
    //    {
    //        public Task Handle(EntityCreateDomainEvent2<IEntity> notification, CancellationToken cancellationToken)
    //        {
    //            if (notification.Is<IStateEntity>())
    //            {
    //                notification.As<IStateEntity>().State.CreatedDate = new DateTimeEpoch();
    //            }

    //            if (notification.Is<IVersionIdentified>())
    //            {
    //                notification.As<IVersionIdentified>().UpdateVersionIdentifier();
    //            }

    //            return null;
    //        }
    //    }
}
