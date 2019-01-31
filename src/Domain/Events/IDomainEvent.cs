namespace Naos.Core.Domain
{
    using MediatR;

    public interface IDomainEvent : INotification
    {
        string Identifier { get; }
    }
}
