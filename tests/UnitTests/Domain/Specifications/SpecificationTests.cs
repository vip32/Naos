namespace Naos.Core.UnitTests.Domain.Specifications
{
    using Naos.Foundation.Domain;
    using Shouldly;
    using Xunit;

    public class SpecificationTests
    {
        [Fact]
        public void ToString_Test()
        {
            new Specification<StubEntity>(e => e.FirstName == "John").ToString()
                .ShouldBe("(FirstName == \"John\")");
        }

        [Fact]
        public void ExpressionCtorIsSatisfiedBy_Test()
        {
            new Specification<StubEntity>(e => e.FirstName == "John")
                .IsSatisfiedBy(new StubEntity { FirstName = "John" })
                .ShouldBe(true);

            new Specification<StubEntity>(e => e.Age == long.MaxValue)
                .IsSatisfiedBy(new StubEntity { FirstName = "John", Age = long.MaxValue })
                .ShouldBe(true);
        }

        [Fact]
        public void StringCtorIsSatisfiedBy_Test()
        {
            // name less parsing is supported
            new Specification<StubEntity>("(FirstName == \"John\")")
                .IsSatisfiedBy(new StubEntity { FirstName = "John" })
                .ShouldBe(true);

            // property casing does not care
            new Specification<StubEntity>("(firstname == \"John\")")
                .IsSatisfiedBy(new StubEntity { FirstName = "John" })
                .ShouldBe(true);

            new Specification<StubEntity>($"(Age == {long.MaxValue})")
                .IsSatisfiedBy(new StubEntity { FirstName = "John", Age = long.MaxValue })
                .ShouldBe(true);

            // it. name is understoord by the string > expression parser (System.Linq.Dynamic)
            new Specification<StubEntity>("(it.FirstName == \"John\")")
                .IsSatisfiedBy(new StubEntity { FirstName = "John" })
                .ShouldBe(true);
        }

        [Fact]
        public void IsNotSatisfiedBy_Test()
        {
            new Specification<StubEntity>(e => e.FirstName == "John")
                .IsSatisfiedBy(new StubEntity { FirstName = "Johny" })
                .ShouldBe(false);
        }

        public class StubEntity
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public long Age { get; set; }
        }
    }
}
