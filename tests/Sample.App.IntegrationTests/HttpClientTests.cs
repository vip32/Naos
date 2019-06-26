namespace Naos.Sample.App.IntegrationTests.Messaging.Infrastructure.Azure
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Http;
    using Naos.Core.Configuration.App;
    using Naos.Core.RequestCorrelation.App.Web;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using Shouldly;
    using Xunit;

    public class HttpClientTests : BaseTest
    {
        private readonly IServiceCollection services = new ServiceCollection();

        public HttpClientTests()
        {
            var configuration = NaosConfigurationFactory.Create();

            this.services.AddTransient<HttpClientCorrelationHandler>();
            this.services.AddTransient<HttpClientLogHandler>();

            this.services
                .AddHttpClient("default")
                    .AddHttpMessageHandler<HttpClientCorrelationHandler>();
            //.AddHttpMessageHandler<HttpClientLogHandler>();
            this.services.Replace(Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, HttpClientLogHandlerBuilderFilter>());

            this.services
                .AddNaos(configuration, "Product", "Capability", new[] { "All" }, n => n
                    .AddRequestCorrelation()
                    .AddOperations(o => o
                        .AddLogging(correlationId: $"TEST{IdGenerator.Instance.Next}")));

            this.ServiceProvider = this.services.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; private set; }

        [Fact]
        public void CanInstantiate_Test()
        {
            var sut = this.ServiceProvider.GetService<IHttpClientFactory>().CreateClient("default");

            sut.ShouldNotBeNull();
        }

        [Fact]
        public async Task CanUseHttpClient_Test()
        {
            var sut = this.ServiceProvider.GetService<IHttpClientFactory>().CreateClient("default");

            var response = await sut.PostAsync("http://mockbin.org/request", null).AnyContext();

            sut.ShouldNotBeNull();
            response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
            response.GetCorrelationIdHeader().ShouldBeEmpty(); // only available in api context
            response.GetRequestIdHeader().ShouldNotBeEmpty();
        }
    }
}
