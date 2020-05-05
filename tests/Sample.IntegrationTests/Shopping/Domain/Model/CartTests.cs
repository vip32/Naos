namespace Naos.Sample.IntegrationTests.Shopping.Domain
{
    using System;
    using Naos.Sample.Shopping.Domain;
    using Shouldly;
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
                DefaultCartId.ShouldBe(@event.AggregateId);
                DefaultCustomerId.ShouldBe(@event.CustomerId);
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
                DefaultCartId.ShouldBe(@event.AggregateId);
                DefaultProductId.ShouldBe(@event.ProductId);
                @event.Quantity.ShouldBe(2);
                @event.AggregateVersion.ShouldBe(0);
            });
        }

        [Fact]
        public void GivenACartWithAProductWhenAddingTheSameProductThenThrowsCartException()
        {
            var cart = new Cart(DefaultCartId, DefaultCustomerId);

            cart.AddProduct(DefaultProductId, 2);
            this.ClearUncommittedEvents(cart);

            Assert.Throws<CartException>(() => cart.AddProduct(DefaultProductId, 1));
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
                DefaultProductId.ShouldBe(@event.ProductId);
                DefaultCartId.ShouldBe(@event.AggregateId);
                @event.OldQuantity.ShouldBe(2);
                @event.NewQuantity.ShouldBe(3);
                @event.AggregateVersion.ShouldBe(1);
            });
        }

        [Fact]
        public void GivenACartWhenChangingQuantityOfAMissingProductThenThrowsCartException()
        {
            var cart = new Cart(DefaultCartId, DefaultCustomerId);

            ShouldThrowExtensions.ShouldThrow<CartException>(
                () => cart.ChangeProductQuantity(DefaultProductId, 3));
        }

        [Fact]
        public void GivenAnEmptyCarWhenAddingAProductAndRequestedQuantityIsGreaterThanProductThresholdThenThrowsCartException()
        {
            var cart = new Cart(DefaultCartId, DefaultCustomerId);

            ShouldThrowExtensions.ShouldThrow<CartException>(
                () => cart.AddProduct(DefaultProductId, 51));
        }

        [Fact]
        public void GivenACartWithAProductWhenRequestedQuantityIsGreaterThanProductThresholdThenThrowsCartException()
        {
            var cart = new Cart(DefaultCartId, DefaultCustomerId);

            cart.AddProduct(DefaultProductId, 1);
            ShouldThrowExtensions.ShouldThrow<CartException>(
                () => cart.ChangeProductQuantity(DefaultProductId, 51));
        }
    }
}
