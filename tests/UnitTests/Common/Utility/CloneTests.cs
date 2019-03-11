namespace Naos.Core.UnitTests.Common.Utility
{
    using Naos.Core.Common;
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

            stub.Clone(mode: Extensions.CloneMode.Json).ShouldNotBeNull();
            stub.Clone(mode: Extensions.CloneMode.Json).FirstName.ShouldBe("John");
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
