namespace Naos.Core.Domain
{
    using MediatR;

    public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
        where TEvent : IDomainEvent
    {
    }
}
