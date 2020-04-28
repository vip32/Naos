namespace Naos.Foundation.Domain
{
    using MediatR;

    public interface IDomainEvent : INotification
    {
        /// <summary>
        /// The identifier of the event
        /// </summary>
        string EventId { get; set; }

        /// <summary>
        /// The correlation id of the event
        /// </summary>
        string CorrelationId { get; set; }
    }

    public interface IDomainEvent<TId> : IDomainEvent
    {
        /// <summary>
        /// The identifier of the aggregate which has generated the event
        /// </summary>
        TId AggregateId { get; }

        /// <summary>
        /// The version of the aggregate when the event has been generated
        /// </summary>
        long AggregateVersion { get; }
    }
}
