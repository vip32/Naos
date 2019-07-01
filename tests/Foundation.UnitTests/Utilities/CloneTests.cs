namespace Naos.Foundation.UnitTests.Utilities
{
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class CloneTests
    {
        [Fact]
        public void CanJsonCloneInstance()
        {
            var stub = new Stub
            {
                FirstName = "John",
                LastName = "Doe"
            };

            stub.Clone(mode: UtilityExtensions.CloneMode.Json).ShouldNotBeNull();
            stub.Clone(mode: UtilityExtensions.CloneMode.Json).FirstName.ShouldBe("John");
        }

        [Fact]
        public void CanBsonCloneInstance()
        {
            var stub = new Stub
            {
                FirstName = "John",
                LastName = "Doe"
            };

            stub.Clone().ShouldNotBeNull();
            stub.Clone().FirstName.ShouldBe("John");
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
