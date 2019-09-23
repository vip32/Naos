namespace Naos.Sample.App.IntegrationTests.Operations.Infrastructure.Azure.LogAnalytics
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Configuration.App;
    using Naos.Foundation;
    using Naos.Operations.Domain;
    using Shouldly;
    using Xunit;

    public class LogEventRepositoryTests : BaseTest
    {
        private readonly ILogEventRepository sut;

        public LogEventRepositoryTests()
        {
            var configuration = NaosConfigurationFactory.Create();

            this.Services
                .AddNaos(configuration, "Product", "Capability", new[] { "All" }, n => n
                    .AddOperations(o => o
                        .AddLogging(
                            l => l.UseAzureLogAnalytics(), // registers ILogEventRepository
                            correlationId: $"TEST{IdGenerator.Instance.Next}"))
                    .AddJobScheduling());

            this.ServiceProvider = this.Services.BuildServiceProvider();
            this.sut = this.ServiceProvider.GetService<ILogEventRepository>();
        }

        [Fact]
        public async Task FindAllAsync_Test()
        {
            var result = await this.sut.FindAllAsync().AnyContext();

            result.ShouldNotBeNull();
            result.ShouldNotBeEmpty();
        }
    }
}
