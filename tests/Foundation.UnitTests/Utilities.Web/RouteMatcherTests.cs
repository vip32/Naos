namespace Naos.Foundation.UnitTests.Utilities
{
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class RouteMatcherTests
    {
        [Fact]
        public void VariousMatches()
        {
            // arrange
            var sut = new RouteMatcher();

            // act/assert
            sut.Match("/api/customers", "/api/customers").ShouldNotBeNull();
            sut.Match("/api/customers", "/api/customers/1234567").ShouldBeNull();
            sut.Match("/api/customers/{id}", "/api/customers/1234567").ShouldNotBeNull();
            sut.Match("/api/customers/{id}", "/api/customers/1234567").ContainsKey("id").ShouldBeTrue();
            sut.Match("/api/customers/{id}", "/api/customers/1234567")["id"].ShouldBe("1234567");
            sut.Match("/api/customers/{id}", "/api/customers/1234567/orders").ShouldBeNull();
            sut.Match("/api/customers/{id}/orders", "/api/customers/1234567/orders").ShouldNotBeNull();
            sut.Match("/api/customers/{id}/orders/{orderId}", "/api/customers/1234567/orders/778899").ShouldNotBeNull();
            sut.Match("/api/customers/{id}/orders/{orderId}", "/api/customers/1234567/orders/778899")["orderId"].ShouldBe("778899");
        }
    }
}
