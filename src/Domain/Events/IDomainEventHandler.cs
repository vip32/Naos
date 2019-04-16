namespace Naos.Core.Domain
{
    using MediatR;

    public interface IDomainEventHandler<in TEvent> : INotificationHandler<TEvent>
        where TEvent : IDomainEvent
    {
        /// <summary>
        /// Determines whether this instance can handle the specified notification.
        /// </summary>
        /// <param name="notification">The notification.</param>
        /// <returns>
        ///   <c>true</c> if this instance can handle the specified notification; otherwise, <c>false</c>.
        /// </returns>
        bool CanHandle(TEvent notification);
    }
}
