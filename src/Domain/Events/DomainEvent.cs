namespace Naos.Core.Domain
{
    using Naos.Core.Common;

    public class DomainEvent : IDomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEvent"/> class.
        /// </summary>
        public DomainEvent()
        {
            this.Id = IdGenerator.Instance.Next;
        }

        public string Id { get; }
    }
}
