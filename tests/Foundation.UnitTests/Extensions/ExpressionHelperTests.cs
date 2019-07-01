namespace Naos.Foundation.UnitTests.Extensions
{
    using System;
    using System.Linq.Expressions;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class ExpressionHelperTests
    {
        [Fact]
        public void ToExpressionString_Test()
        {
            Expression<Func<StubEntity, bool>> result1 = (e) => e.FirstName == "John";
            result1.ToExpressionString()
                .ShouldBe("(FirstName == \"John\")");

            Expression<Func<StubEntity, bool>> result2 = (e) => e.Age == 12;
            result2.ToExpressionString()
                .ShouldBe("(Age == 12)");
        }

        [Fact]
        public void FromExpressionString_Test()
        {
            ExpressionHelper.FromExpressionString<StubEntity>("(FirstName == \"John\")")
                .ToExpressionString()
                .ShouldBe("(FirstName == \"John\")");

            ExpressionHelper.FromExpressionString<StubEntity>("(firstname == \"John\")")
                .ToExpressionString()
                .ShouldBe("(FirstName == \"John\")");

            ExpressionHelper.FromExpressionString<StubEntity>("(Age == 12)")
                .ToExpressionString()
                .ShouldBe("(Age == 12)");
        }

        public class StubEntity
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public int Age { get; set; }
        }
    }
}
