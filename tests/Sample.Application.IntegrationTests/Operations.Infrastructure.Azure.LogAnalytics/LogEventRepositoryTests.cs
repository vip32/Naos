namespace Naos.Sample.Application.IntegrationTests.Operations.Infrastructure.Azure.LogAnalytics
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Foundation;
    using Naos.Operations.Domain;
    using Shouldly;
    using Xunit;

    public class LogEventRepositoryTests : BaseTest
    {
        private readonly ILogEventRepository sut;

        public LogEventRepositoryTests()
        {
            this.Services
                .AddNaos("Product", "Capability", new[] { "All" }, n => n
                    .AddOperations(o => o
                        .AddLogging(
                            l => l.UseAzureLogAnalytics(), // registers loganalytics sink
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
