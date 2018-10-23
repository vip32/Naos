//namespace Naos.Core.Domain
//{
//    using System.Threading;
//    using System.Threading.Tasks;
//    using Naos.Core.Common;

//    public class EntityUpdateDomainEventDomainEventHandler<T> : IDomainEventHandler<EntityUpdateDomainEvent<IEntity>>
//    {
//        public Task Handle(EntityUpdateDomainEvent<IEntity> notification, CancellationToken cancellationToken)
//        {
//            if (notification.Is<IStateEntity>())
//            {
//                notification.As<IStateEntity>().State.UpdatedDate = new DateTimeEpoch();
//            }

//            if (notification.Is<IVersionIdentified>())
//            {
//                notification.As<IVersionIdentified>().UpdateVersionIdentifier();
//            }

//            return null;
//        }
//    }
//}
