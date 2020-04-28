namespace Naos.Sample.Shopping.Domain
{
    using System;
    using EnsureThat;

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
}
