namespace Naos.Foundation.UnitTests.Utilities
{
    using Microsoft.AspNetCore.Http.Internal;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class RouteMatcherTests
    {
        [Fact]
        public void VariousPathMatches()
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

        [Fact]
        public void VariousPathAndQueryStringMatches()
        {
            // arrange
            var sut = new RouteMatcher();

            // act/assert
            var query = new QueryCollection(Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery("?sorted=date"));
            var noopQuery = new QueryCollection(Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery("?sorted=noop"));
            sut.Match("/api/customers/{id}/orders?sorted=date", "/api/customers/1234567/orders", query).ShouldNotBeNull();
            sut.Match("/api/customers/{id}/orders?sorted=date", "/api/customers/1234567/orders?sorted=date", query).ShouldNotBeNull();
            sut.Match("/api/customers/{id}/orders?sorted=date", "/api/customers/1234567/orders").ShouldBeNull();
            sut.Match("/api/customers/{id}/orders?sorted=date", "/api/customers/1234567/orders", noopQuery).ShouldBeNull();
            sut.Match("/api/customers/{id}/orders?sorted=date", "/api/customers/1234567/orders", query).ContainsKey("id").ShouldBeTrue();
            sut.Match("/api/customers/{id}/orders?sorted=date", "/api/customers/1234567/orders", query)["id"].ShouldBe("1234567");
            //sut.Match("/api/customers/{id}/orders?orderId={orderId}", "/api/customers/1234567/orders?orderId=778899", query).ContainsKey("orderId").ShouldBeTrue();
            //sut.Match("/api/customers/{id}/orders?orderId={orderId}", "/api/customers/1234567/orders?orderId=778899", query)["orderId"].ShouldBe("778899");
            //sut.Match("/api/customers/{id}/orders/{orderId}", "/api/customers/1234567/orders/778899")["orderId"].ShouldBe(778899);
        }
    }
}
