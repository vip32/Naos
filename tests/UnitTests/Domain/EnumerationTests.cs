namespace Naos.UnitTests.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Naos.Foundation.Domain;
    using Shouldly;
    using Xunit;

    public class EnumerationTests
    {
        [Fact]
        public void GetAllTest()
        {
            // arrange/act
            var sut = Enumeration.GetAll<StubEnumeration>();

            // assert
            sut.ShouldNotBeEmpty();
            sut.Count().ShouldBe(3);
        }

        [Fact]
        public void GetByIdTest()
        {
            // arrange/act
            var sut = Enumeration.From<StubEnumeration>(2);

            // assert
            sut.ShouldNotBeNull();
            sut.Id.ShouldBe(2);
        }

        [Fact]
        public void GetByInvalidIdTest()
        {
            // arrange/act/assert
            Should.Throw<InvalidOperationException>(() => Enumeration.From<StubEnumeration>(0))
                .Message.ShouldBe("'0' is not a valid id for Naos.UnitTests.Domain.StubEnumeration");
        }

        [Fact]
        public void GetByNameTest()
        {
            // arrange/act
            var sut = Enumeration.From<StubEnumeration>("Stub03");

            // assert
            sut.ShouldNotBeNull();
            sut.Id.ShouldBe(3);
        }

        [Fact]
        public void GetByInvalidNameTest()
        {
            // arrange/act/assert
            Should.Throw<InvalidOperationException>(() => Enumeration.From<StubEnumeration>("Stub00"))
                .Message.ShouldBe("'Stub00' is not a valid name for Naos.UnitTests.Domain.StubEnumeration");
        }

        [Fact]
        public void EqualsTest()
        {
            // arrange/act
            var sut = Enumeration.From<StubEnumeration>("Stub03")
                .Equals(Enumeration.From<StubEnumeration>("Stub03"));

            // assert
            sut.ShouldBeTrue();
        }

        [Fact]
        public void NotEqualsTest()
        {
            // arrange/act
            var sut = Enumeration.From<StubEnumeration>("Stub01")
                .Equals(Enumeration.From<StubEnumeration>("Stub03"));

            // assert
            sut.ShouldBeFalse();
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class StubEnumeration : Enumeration
#pragma warning restore SA1402 // File may only contain a single type
    {
        public static StubEnumeration Stub01 = new(1, "Stub01");
        public static StubEnumeration Stub02 = new(2, "Stub02");
        public static StubEnumeration Stub03 = new(3, "Stub03");

        public StubEnumeration(int id, string name)
            : base(id, name)
        {
        }

        public static IEnumerable<StubEnumeration> GetAll() => GetAll<StubEnumeration>();
    }
}
