namespace Naos.Core.Domain
{
    using Naos.Core.Common;

    public class DomainEvent : IDomainEvent
    {
        public DomainEvent()
        {
            this.Identifier = RandomGenerator.GenerateString(5, false);
        }

        public string Identifier { get; }
    }
}
