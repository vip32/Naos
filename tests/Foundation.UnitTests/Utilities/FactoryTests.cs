namespace Naos.Foundation.UnitTests.Utilities
{
    using System.Collections.Generic;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class FactoryTests
    {
        [Fact]
        public void CanCreateInstance()
        {
            Factory<Stub>.Create().ShouldNotBeNull();
            Factory<Stub>.Create("firstname").ShouldNotBeNull();
            Factory<Stub>.Create("firstname", "NOARG").ShouldBeNull();
            Factory.Create(typeof(Stub)).ShouldNotBeNull();
            Factory.Create(typeof(Stub), "firstname").ShouldNotBeNull();
            Factory.Create(typeof(Stub), "firstname", "NOARG").ShouldBeNull();
        }

        [Fact]
        public void CanCreateWithDictionaryProperties1Instance()
        {
            // arrange
            var properties = new Dictionary<string, object>
            {
                ["Firstname"] = "John",
                ["lastname"] = "Doe"
            };

            // act
            var sut = Factory<Stub>.Create(properties);

            // assert
            sut.FirstName.ShouldBe("John");
            sut.LastName.ShouldBe("Doe");
        }

        [Fact]
        public void CanCreateWithDictionaryProperties2Instance()
        {
            // arrange
            var properties = new Dictionary<string, object>
            {
                ["Firstname"] = "John",
                ["lastname"] = "Doe"
            };

            // act
            var sut = Factory.Create(typeof(Stub), properties) as Stub;

            // assert
            sut.FirstName.ShouldBe("John");
            sut.LastName.ShouldBe("Doe");
        }

        public class Stub
        {
            public Stub()
            {
            }

            public Stub(string firstName)
            {
                this.FirstName = firstName;
            }

            public string FirstName { get; set; }

            public string LastName { get; set; }
        }
    }
}
