namespace Naos.Foundation.UnitTests.Domain
{
    using System;
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

            instance0.Equals(instance1).ShouldBeTrue(); //IEquatable
            instance0.ShouldBe(instance1);
            instance1.ShouldBe(instance1);
            instance1.ShouldNotBe(instance2);
            instance0.Equals(instance2).ShouldBeFalse(); // IEquatable
        }

        [Fact]
        public void ComparableTests()
        {
            var instance0 = new StubValueObjectComparable
            {
                StreetName = "One Microsoft Way",
                HouseNumber = 1,
                City = "Seattle",
                ZipCode = 2222
            };

            var instance1 = new StubValueObjectComparable
            {
                StreetName = "One Microsoft Way",
                HouseNumber = 1,
                City = "Seattle",
                ZipCode = 3333
            };

            var instance2 = new StubValueObjectComparable
            {
                StreetName = "One Microsoft Way",
                HouseNumber = 1,
                City = "New York",
                ZipCode = 1111
            };

            var instance3 = new StubValueObjectComparable
            {
                StreetName = "Second Microsoft Way",
                HouseNumber = 10,
                City = "New York",
                ZipCode = 1111
            };

            instance0.Equals(instance1).ShouldBeTrue(); //IEquatable
            instance0.ShouldBe(instance1);
            instance1.ShouldBe(instance1);
            instance1.ShouldNotBe(instance2);
            instance0.Equals(instance2).ShouldBeFalse(); // IEquatable
            (instance0 > instance2).ShouldBeTrue(); // IComparable
            (instance2 > instance1).ShouldBeFalse(); // IComparable
            (instance2 == instance3).ShouldBeTrue(); // IComparable
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

        public class StubValueObjectComparable : ValueObjectComparable
        {
            public string StreetName { get; set; }

            public int HouseNumber { get; set; }

            public string City { get; set; }

            public int ZipCode { get; set; }

            protected override IEnumerable<object> GetAtomicValues()
            {
                yield return this.StreetName;
                yield return this.HouseNumber;
                yield return this.City;
            }

            protected override IEnumerable<IComparable> GetComparableAtomicValues()
            {
                yield return this.ZipCode;
            }
        }
    }
}
