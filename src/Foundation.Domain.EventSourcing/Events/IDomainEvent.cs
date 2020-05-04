namespace Naos.Foundation.Domain
{
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
