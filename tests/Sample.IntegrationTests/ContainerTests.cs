namespace Naos.Sample.IntegrationTests
{
    using Xunit;

    public class ContainerTests : BaseTest
    {
        [Fact]
        public void Verify_Test()
        {
            // assert
            this.container.Verify();
        }
    }
}
