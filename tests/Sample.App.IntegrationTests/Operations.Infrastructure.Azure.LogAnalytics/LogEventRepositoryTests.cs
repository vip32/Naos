namespace Naos.Sample.App.IntegrationTests.Messaging.Infrastructure.Azure
{
    using Naos.Core.App.Configuration;
    using Naos.Core.App.Operations.Serilog;
    using Naos.Core.Operations.Domain.Repositories;
    using Naos.Core.Operations.Infrastructure.Azure.LogAnalytics;
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
            this.container.Verify();
        }

        [Fact]
        public void VerifyContainer()
        {
            this.container.Verify();
        }

        [Fact]
        public void CanInstantiateLogEventRepository()
        {
            var sut = this.container.GetInstance<ILogEventRepository>();

            Assert.NotNull(sut);
        }
    }
}
