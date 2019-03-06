namespace Naos.Core.UnitTests.Common.Utility
{
    using Naos.Core.Common;
    using Xunit;

    public class FactoryTests
    {
        [Fact]
        public void CanCreateInstace()
        {
            var result = Factory<Stub>.Create();

            Assert.NotNull(result);
        }

        public class Stub
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }
    }
}
