namespace Naos.Foundation.UnitTests.Utilities
{
    using System;
    using System.Linq.Expressions;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class ExpressionHelperTests
    {
        [Fact]
        public void GetPropertyName_Test()
        {
            // arrange
            Expression<Func<StubObject, bool>> expr1 = t => t.FirstName == "John";
            Expression<Func<StubObject, bool>> expr2 = t => t.Active;
#pragma warning disable RCS1033 // Remove redundant boolean literal.
            Expression<Func<StubObject, bool>> expr3 = t => t.Active == true;
#pragma warning restore RCS1033 // Remove redundant boolean literal.

            // act/assert
            ExpressionHelper.GetPropertyName(expr1).ShouldBe("FirstName");
            ExpressionHelper.GetPropertyName(expr2).ShouldBe("Active");
            ExpressionHelper.GetPropertyName(expr3).ShouldBe("Active");
        }

        public class StubObject
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public bool Active { get; set; }
        }
    }
}
