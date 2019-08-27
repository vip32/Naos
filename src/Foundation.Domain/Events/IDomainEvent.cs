namespace Naos.Foundation.Domain
{
    using MediatR;

    public interface IDomainEvent : INotification
    {
        string Id { get; set; }

        string CorrelationId { get; set; }
    }
}
