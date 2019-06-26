namespace Naos.Core.UnitTests.Common.Criteria
{
    using Naos.Foundation;
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
        public void IsNumberValue_Test()
        {
            new Criteria("FirstName", CriteriaOperator.Equal, "John")
                .IsNumberValue().ShouldBe(false);

            new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, 21)
                .IsNumberValue().ShouldBe(true);

            new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, 21.8)
                .IsNumberValue().ShouldBe(true);

            new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, "22")
                .IsNumberValue().ShouldBe(true);

            new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, "22.8")
                .IsNumberValue().ShouldBe(true);

            new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, long.MaxValue)
                .IsNumberValue().ShouldBe(true);
        }

        //[Fact]
        //public void IsType_Test()
        //{
        //    new Criteria("FirstName", CriteriaOperator.Equal, "John")
        //        .IsStringValue().ShouldBe(true);

        //    new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, 21)
        //        .IsIntValue().ShouldBe(true);

        //    //new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, 21.8)
        //    //    .IsDecimalValue().ShouldBe(true);

        //    new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, "22")
        //        .IsIntValue().ShouldBe(true);

        //    //new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, "22.8")
        //    //    .IsDecimalValue().ShouldBe(true);

        //    new Criteria("Age", CriteriaOperator.GreaterThanOrEqual, long.MaxValue)
        //        .IsLongValue().ShouldBe(true);
        //}

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
