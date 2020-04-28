namespace Naos.Sample.Shopping.Domain
{
    using System;
    using Naos.Foundation.Domain;

    public class ProductQuantityChangedEvent : DomainEventBase<Guid>
    {
        internal ProductQuantityChangedEvent(Guid productId, int oldQuantity, int newQuantity)
            : base()
        {
            this.ProductId = productId;
            this.OldQuantity = oldQuantity;
            this.NewQuantity = newQuantity;
        }

        private ProductQuantityChangedEvent()
        {
        }

        private ProductQuantityChangedEvent(Guid aggregateId, long aggregateVersion, Guid productId,
            int oldQuantity, int newQuantity)
            : base(aggregateId, aggregateVersion)
        {
            this.ProductId = productId;
            this.OldQuantity = oldQuantity;
            this.NewQuantity = newQuantity;
        }

        public Guid ProductId { get; private set; }

        public int OldQuantity { get; private set; }

        public int NewQuantity { get; private set; }

        public override IDomainEvent<Guid> WithAggregate(Guid aggregateId, long aggregateVersion)
        {
            return new ProductQuantityChangedEvent(aggregateId, aggregateVersion, this.ProductId, this.OldQuantity, this.NewQuantity);
        }
    }
}
