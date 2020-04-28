#pragma warning disable SA1402 // File may only contain a single type
namespace Naos.Sample.Shopping.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using EnsureThat;
    using Naos.Foundation.Domain;

    public class Cart : EventSourcingAggregateRoot<Guid>
    {
        public const int ProductQuantityThreshold = 50;

        public Cart(Guid cartId, Guid customerId)
            : this()
        {
            EnsureArg.IsNotDefault(cartId, nameof(cartId));
            EnsureArg.IsNotDefault(customerId, nameof(customerId));

            this.RaiseEvent(new CartCreatedEvent(cartId, customerId));
        }

        //Needed for persistence purposes
        private Cart()
        {
            this.Items = new List<CartItem>();
        }

        private Guid CustomerId { get; set; }

        private List<CartItem> Items { get; set; }

        public void AddProduct(Guid productId, int quantity)
        {
            EnsureArg.IsNotDefault(productId, nameof(productId));

            if (this.ContainsProduct(productId))
            {
                throw new CartException($"product {productId} already added");
            }

            this.CheckQuantity(productId, quantity);
            this.RaiseEvent(new ProductAddedEvent(productId, quantity));
        }

        public void ChangeProductQuantity(Guid productId, int quantity)
        {
            if (!this.ContainsProduct(productId))
            {
                throw new CartException($"Product {productId} not found");
            }

            this.CheckQuantity(productId, quantity);
            this.RaiseEvent(new ProductQuantityChangedEvent(productId, this.GetCartItemByProduct(productId).Quantity, quantity));
        }

        public override string ToString()
        {
            return $"{{ Id: \"{this.Id}\", CustomerId: \"{this.CustomerId.ToString()}\", Items: [{string.Join(", ", this.Items.Select(x => x.ToString()))}] }}";
        }

        public /*internal*/ void Apply(CartCreatedEvent @event)
        {
            EnsureArg.IsNotNull(@event, nameof(@event));

            this.Id = @event.AggregateId;
            this.CustomerId = @event.CustomerId;
        }

        public /*internal*/ void Apply(ProductAddedEvent @event)
        {
            EnsureArg.IsNotNull(@event, nameof(@event));

            this.Items.Add(new CartItem(@event.ProductId, @event.Quantity));
        }

        public /*internal*/ void Apply(ProductQuantityChangedEvent @event)
        {
            EnsureArg.IsNotNull(@event, nameof(@event));

            var item = this.Items.Single(x => x.ProductId == @event.ProductId);

            this.Items.Remove(item);
            this.Items.Add(item.WithQuantity(@event.NewQuantity));
        }

        private void CheckQuantity(Guid productId, int quantity)
        {
            EnsureArg.IsNotDefault(@productId, nameof(@productId));

            if (quantity <= 0)
            {
                throw new ArgumentException("quantity must be greater than zero", nameof(quantity));
            }

            if (quantity > ProductQuantityThreshold)
            {
                throw new CartException($"quantity for product {productId} must be less than or equal to {ProductQuantityThreshold}");
            }
        }

        private bool ContainsProduct(Guid productId)
        {
            EnsureArg.IsNotDefault(@productId, nameof(@productId));

            return this.Items.Any(x => x.ProductId == productId);
        }

        private CartItem GetCartItemByProduct(Guid productId)
        {
            EnsureArg.IsNotDefault(@productId, nameof(@productId));

            return this.Items.Single(x => x.ProductId == productId);
        }
    }

    public class CartItem // TODO: is valueobject or entity?
    {
        public CartItem(Guid productId, int quantity)
        {
            EnsureArg.IsNotDefault(productId, nameof(productId));

            this.ProductId = productId;
            this.Quantity = quantity;
        }

        public Guid ProductId { get; }

        public int Quantity { get; }

        public override string ToString()
        {
            return $"{{ ProductId: \"{this.ProductId}\", Quantity: {this.Quantity} }}";
        }

        public CartItem WithQuantity(int quantity)
        {
            return new CartItem(this.ProductId, quantity);
        }
    }

    public class CartCreatedEvent : DomainEventBase<Guid>
    {
        internal CartCreatedEvent(Guid aggregateId, Guid customerId)
            : base(aggregateId)
        {
            this.CustomerId = customerId;
        }

        private CartCreatedEvent()
        {
        }

        private CartCreatedEvent(Guid aggregateId, long aggregateVersion, Guid customerId)
            : base(aggregateId, aggregateVersion)
        {
            this.CustomerId = customerId;
        }

        public Guid CustomerId { get; private set; }

        public override IDomainEvent<Guid> WithAggregate(Guid aggregateId, long aggregateVersion)
        {
            return new CartCreatedEvent(aggregateId, aggregateVersion, this.CustomerId);
        }
    }

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

    [Serializable]
    public class CartException : Exception
    {
        public CartException()
        {
        }

        public CartException(string message)
            : base(message)
        {
        }

        public CartException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CartException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
