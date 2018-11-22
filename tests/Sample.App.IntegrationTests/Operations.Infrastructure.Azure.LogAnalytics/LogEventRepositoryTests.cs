namespace Naos.Sample.App.IntegrationTests.Messaging.Infrastructure.Azure
{
    using System.Threading.Tasks;
    using Naos.Core.App.Configuration;
    using Naos.Core.App.Operations.Serilog;
    using Naos.Core.Operations.Domain.Repositories;
    using Naos.Core.Operations.Infrastructure.Azure.LogAnalytics;
    using Shouldly;
    using SimpleInjector;
    using Xunit;

    public class LogEventRepositoryTests : BaseTest
    {
        private readonly Container container = new Container();

        public LogEventRepositoryTests()
        {
            var configuration = NaosConfigurationFactory.CreateRoot();
            this.container = new Container()
                .AddNaosLogging(configuration)
                .AddNaosOperations(configuration);
        }

        [Fact]
        public void VerifyContainer_Test()
        {
            this.container.Verify();
        }

        [Fact]
        public void CanInstantiate_Test()
        {
            var sut = this.container.GetInstance<ILogEventRepository>();

            sut.ShouldNotBeNull();
        }

        [Fact]
        public async Task FindAllAsync_Test()
        {
            var sut = this.container.GetInstance<ILogEventRepository>();
            var result = await sut.FindAllAsync().ConfigureAwait(false);

            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }
    }
}
