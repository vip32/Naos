namespace Naos.Core.UnitTests.RequestFiltering.App.Model
{
    using Naos.Core.Common;
    using Naos.Core.RequestFiltering.App;
    using Shouldly;
    using Xunit;

    public class CriteriaTests
    {
        [Fact]
        public void ToString_Test()
        {
            new Criteria("FirstName", CriteriaOperator.Equal, "John")
                .ToString().ShouldBe("(FirstName==\"John\")");

            new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, 21)
                .ToString().ShouldBe("(Age>=21)");
        }

        [Fact]
        public void ToExpression_Test()
        {
            new Criteria("FirstName", CriteriaOperator.Equal, "John")
                .ToExpression<StubEntity>().ToString()
                .ShouldBe(ExpressionHelper.FromExpressionString<StubEntity>("(FirstName==\"John\")").ToString());

            new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, 21)
                .ToExpression<StubEntity>().ToString()
                .ShouldBe(ExpressionHelper.FromExpressionString<StubEntity>("(Age>=21)").ToString());
        }

        [Fact]
        public void IsNumeric_Test()
        {
            new Criteria("FirstName", CriteriaOperator.Equal, "John")
                .IsNumeric.ShouldBe(false);

            new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, 21)
                .IsNumeric.ShouldBe(true);

            new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, "21")
                .IsNumeric.ShouldBe(true);

            new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, long.MaxValue)
                .IsNumeric.ShouldBe(true);
        }

        public class StubEntity
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public long Age { get; set; }
        }
    }
}


//Param_0 => (Param_0.FirstName == "John")
//    but was
//Param_0 => (Param_0.FirstName == "John")
