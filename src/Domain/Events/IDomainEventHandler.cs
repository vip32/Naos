namespace Naos.Core.Domain
{
    using MediatR;

    public interface IDomainEventHandler<in T> : INotificationHandler<T>
        where T : IDomainEvent
    {
    }
}
