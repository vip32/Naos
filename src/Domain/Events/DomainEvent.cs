namespace Naos.Core.Domain
{
    using Naos.Core.Common;

    public class DomainEvent : IDomainEvent
    {
        public DomainEvent()
        {
            this.Identifier = IdGenerator.Instance.Next;
        }

        public string Identifier { get; }
    }
}
