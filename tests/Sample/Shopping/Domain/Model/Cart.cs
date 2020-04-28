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
}
