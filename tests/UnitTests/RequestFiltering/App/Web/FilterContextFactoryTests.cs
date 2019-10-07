namespace Naos.UnitTests.RequestFiltering.App.Web
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Naos.Foundation;
    using Naos.RequestFiltering.App.Web;
    using NSubstitute;
    using Shouldly;
    using Xunit;

    public class FilterContextFactoryTests
    {
        [Fact]
        public void Create_Tests()
        {
            // arrange
            var logger = Substitute.For<ILogger<FilterContextFactory>>();
            var query = new QueryCollection(new Dictionary<string, StringValues>()
            {
                ["q"] = "FirstName=John,Age=ge:21",
                ["order"] = "FirstName,desc:Age",
                ["skip"] = "5",
                ["take"] = "10",
            });
            var request = Substitute.For<HttpRequest>();
            request.Query.Returns(query);
            var sut = new FilterContextFactory(logger);

            // act
            var result = sut.Create(request);

            // assert
            var criteria1 = result.Criterias.FirstOrDefault(c => c.Name.SafeEquals("FirstName"));
            criteria1.Operator.ShouldBe(CriteriaOperator.Equal);
            criteria1.ShouldNotBeNull();
            criteria1.IsNumberValue().ShouldBe(false);
            criteria1.Value.ShouldBe("John");

            var criteria2 = result.Criterias.FirstOrDefault(c => c.Name.SafeEquals("Age"));
            criteria2.ShouldNotBeNull();
            criteria2.Operator.ShouldBe(CriteriaOperator.GreaterThanOrEqual);
            criteria2.IsNumberValue().ShouldBe(true);
            criteria2.Value.ShouldBe("21");

            var order1 = result.Orders.FirstOrDefault(c => c.Name.SafeEquals("FirstName"));
            order1.ShouldNotBeNull();
            order1.Direction.ShouldBe(Naos.RequestFiltering.App.OrderDirection.Asc);

            var order2 = result.Orders.FirstOrDefault(c => c.Name.SafeEquals("Age"));
            order2.ShouldNotBeNull();
            order2.Direction.ShouldBe(Naos.RequestFiltering.App.OrderDirection.Desc);

            result.Skip.ShouldBe(5);
            result.Take.ShouldBe(10);
        }
    }
}
