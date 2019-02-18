namespace Naos.Sample.App.IntegrationTests.Messaging.Infrastructure.Azure
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Common;
    using Naos.Core.Configuration;
    using Naos.Core.Operations.Domain.Repositories;
    using Shouldly;
    using Xunit;

    public class LogEventRepositoryTests : BaseTest
    {
        private readonly IServiceCollection services = new ServiceCollection();
        private readonly ILogEventRepository sut;

        public LogEventRepositoryTests()
        {
            var configuration = NaosConfigurationFactory.Create();

            this.services
                .AddNaos(configuration, "Product", "Capability", new[] { "All" }, n => n
                    .AddOperations(o => o
                        .AddLogging(
                            l => l.UseAzureLogAnalytics(), // registers ILogEventRepository
                            correlationId: $"TEST{RandomGenerator.GenerateString(9, true)}"))
                    .AddJobScheduling());

            this.ServiceProvider = this.services.BuildServiceProvider();
            this.sut = this.ServiceProvider.GetService<ILogEventRepository>();
        }

        public ServiceProvider ServiceProvider { get; private set; }

        [Fact]
        public async Task FindAllAsync_Test()
        {
            var result = await this.sut.FindAllAsync().AnyContext();

            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task FindAllAsync2_Test()
        {
            var result = await this.sut.FindAllAsync().AnyContext();

            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }
    }
}
