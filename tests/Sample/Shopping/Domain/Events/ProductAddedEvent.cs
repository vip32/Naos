namespace Naos.Sample.Shopping.Domain
{
    using System;
    using Naos.Foundation.Domain;

    public class ProductAddedEvent : DomainEventBase<Guid>
    {
        internal ProductAddedEvent(Guid productId, int quantity)
            : base()
        {
            this.ProductId = productId;
            this.Quantity = quantity;
        }

        internal ProductAddedEvent(Guid aggregateId, long aggregateVersion, Guid productId, int quantity)
            : base(aggregateId, aggregateVersion)
        {
            this.ProductId = productId;
            this.Quantity = quantity;
        }

        private ProductAddedEvent()
        {
        }

        public Guid ProductId { get; private set; }

        public int Quantity { get; private set; }

        public override IDomainEvent<Guid> WithAggregate(Guid aggregateId, long aggregateVersion)
        {
            return new ProductAddedEvent(aggregateId, aggregateVersion, this.ProductId, this.Quantity);
        }
    }
}
