namespace Naos.Core.UnitTests.Domain.Model
{
    using System.Collections.Generic;
    using Naos.Foundation.Domain;
    using Shouldly;
    using Xunit;

    public class ValueObjectTests
    {
        [Fact]
        public void EqualityTests()
        {
            var instance0 = new StubValueObject
            {
                StreetName = "One Microsoft Way",
                HouseNumber = 1,
                City = "Seattle"
            };

            var instance1 = new StubValueObject
            {
                StreetName = "One Microsoft Way",
                HouseNumber = 1,
                City = "Seattle"
            };

            var instance2 = new StubValueObject
            {
                StreetName = "One Microsoft Way",
                HouseNumber = 1,
                City = "New York"
            };

            instance0.ShouldBe(instance1);
            instance1.ShouldBe(instance1);
            instance1.ShouldNotBe(instance2);
        }

        public class StubValueObject : ValueObject
        {
            public string StreetName { get; set; }

            public int HouseNumber { get; set; }

            public string City { get; set; }

            protected override IEnumerable<object> GetAtomicValues()
            {
                yield return this.StreetName;
                yield return this.HouseNumber;
                yield return this.City;
            }
        }
    }
}
