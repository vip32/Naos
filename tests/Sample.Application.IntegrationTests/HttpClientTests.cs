namespace Naos.Sample.Application.IntegrationTests.Messaging.Infrastructure.Azure
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Http;
    using Naos.Foundation;
    using Naos.Foundation.Application;
    using Naos.RequestCorrelation.Application.Web;
    using Shouldly;
    using Xunit;

    public class HttpClientTests : BaseTest
    {
        public HttpClientTests()
        {
            this.Services.AddTransient<HttpClientCorrelationHandler>();
            this.Services.AddTransient<HttpClientLogHandler>();

            this.Services
                .AddHttpClient("default")
                    .AddHttpMessageHandler<HttpClientCorrelationHandler>();
            //.AddHttpMessageHandler<HttpClientLogHandler>();
            this.Services.Replace(Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton<IHttpMessageHandlerBuilderFilter, HttpClientLogHandlerBuilderFilter>());

            this.Services
                .AddNaos("Product", "Capability", new[] { "All" }, n => n
                    .AddRequestCorrelation()
                    .AddOperations(o => o
                        .AddLogging(correlationId: $"TEST{IdGenerator.Instance.Next}")));

            this.ServiceProvider = this.Services.BuildServiceProvider();
        }

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
            response.GetCorrelationIdHeader().ShouldBeNullOrEmpty(); // only available in api context
            response.GetRequestIdHeader().ShouldNotBeEmpty();
        }
    }
}
