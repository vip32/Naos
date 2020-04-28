namespace Naos.Foundation.Domain
{
    using MediatR;

    public interface IDomainEvent : INotification
    {
        string EventId { get; set; }

        string CorrelationId { get; set; }
    }
}
