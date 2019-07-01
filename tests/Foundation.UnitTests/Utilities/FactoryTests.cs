namespace Naos.Foundation.UnitTests.Utilities
{
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class FactoryTests
    {
        [Fact]
        public void CanCreateInstance()
        {
            Factory<Stub>.Create().ShouldNotBeNull();
            Factory<Stub>.Create(typeof(Stub)).ShouldNotBeNull();
            Factory<Stub>.Create(typeof(Stub), "firstname").ShouldNotBeNull();
            Factory<Stub>.Create(typeof(Stub), "firstname", "NOARG").ShouldBeNull();
            Factory.Create(typeof(Stub)).ShouldNotBeNull();
            Factory.Create(typeof(Stub), "firstname").ShouldNotBeNull();
            Factory.Create(typeof(Stub), "firstname", "NOARG").ShouldBeNull();
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
