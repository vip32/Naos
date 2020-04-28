namespace Naos.Sample.IntegrationTests.Shopping.Domain
{
    using System;
    using Naos.Sample.Shopping.Domain;
    using Xunit;

    public class CartTests : GenericAggregateBaseTest<Cart, Guid>
    {
        private static readonly Guid DefaultCustomerId = Guid.NewGuid();
        private static readonly Guid DefaultCartId = Guid.NewGuid();
        private static readonly Guid DefaultProductId = Guid.NewGuid();

        [Fact]
        public void GivenNoCartExistsWhenCreateOneThenCartCreatedEvent()
        {
            var cart = new Cart(DefaultCartId, DefaultCustomerId);

            this.AssertSingleUncommittedEvent<CartCreatedEvent>(cart, @event =>
            {
                Assert.Equal(DefaultCartId, @event.AggregateId);
                Assert.Equal(DefaultCustomerId, @event.CustomerId);
            });
        }

        [Fact]
        public void GivenACartWhenAddAProductThenProductAddedEvent()
        {
            var cart = new Cart(DefaultCartId, DefaultCustomerId);
            this.ClearUncommittedEvents(cart);

            cart.AddProduct(DefaultProductId, 2);

            this.AssertSingleUncommittedEvent<ProductAddedEvent>(cart, @event =>
            {
                Assert.Equal(DefaultProductId, @event.ProductId);
                Assert.Equal(2, @event.Quantity);
                Assert.Equal(DefaultCartId, @event.AggregateId);
                Assert.Equal(0, @event.AggregateVersion);
            });
        }

        [Fact]
        public void GivenACartWithAProductWhenAddingTheSameProductThenThrowsCartException()
        {
            var cart = new Cart(DefaultCartId, DefaultCustomerId);

            cart.AddProduct(DefaultProductId, 2);
            this.ClearUncommittedEvents(cart);

            Assert.Throws<CartException>(() => { cart.AddProduct(DefaultProductId, 1); });
            Assert.Empty(this.GetUncommittedEventsOf(cart));
        }

        [Fact]
        public void GivenACartWithAProductWhenChangingQuantityThenProductQuantityChangedEvent()
        {
            var cart = new Cart(DefaultCartId, DefaultCustomerId);

            cart.AddProduct(DefaultProductId, 2);
            this.ClearUncommittedEvents(cart);
            cart.ChangeProductQuantity(DefaultProductId, 3);
            this.AssertSingleUncommittedEvent<ProductQuantityChangedEvent>(cart, @event =>
            {
                Assert.Equal(DefaultProductId, @event.ProductId);
                Assert.Equal(2, @event.OldQuantity);
                Assert.Equal(3, @event.NewQuantity);
                Assert.Equal(DefaultCartId, @event.AggregateId);
                Assert.Equal(1, @event.AggregateVersion);
            });
        }

        [Fact]
        public void GivenACartWhenChangingQuantityOfAMissingProductThenThrowsCartException()
        {
            var cart = new Cart(DefaultCartId, DefaultCustomerId);

            Assert.Throws<CartException>(() => { cart.ChangeProductQuantity(DefaultProductId, 3); });
        }

        [Fact]
        public void GivenAnEmptyCarWhenAddingAProductAndRequestedQuantityIsGreaterThanProductThresholdThenThrowsCartException()
        {
            var cart = new Cart(DefaultCartId, DefaultCustomerId);

            Assert.Throws<CartException>(() => { cart.AddProduct(DefaultProductId, 51); });
        }

        [Fact]
        public void GivenACartWithAProductWhenRequestedQuantityIsGreaterThanProductThresholdThenThrowsCartException()
        {
            var cart = new Cart(DefaultCartId, DefaultCustomerId);

            cart.AddProduct(DefaultProductId, 1);
            Assert.Throws<CartException>(() => { cart.ChangeProductQuantity(DefaultProductId, 51); });
        }
    }
}
