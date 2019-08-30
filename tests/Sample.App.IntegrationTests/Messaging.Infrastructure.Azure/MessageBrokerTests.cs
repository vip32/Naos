namespace Naos.Sample.App.IntegrationTests.Messaging.Infrastructure.Azure
{
    using System;
    using System.Linq;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Configuration.App;
    using Naos.Core.Messaging;
    using Naos.Foundation;
    using Shouldly;
    using Xunit;

    public class MessageBrokerTests : BaseTest
    {
        private readonly IMessageBroker sut;

        public MessageBrokerTests()
        {
            var configuration = NaosConfigurationFactory.Create();

            this.Services
                .AddMediatR(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.GetName().Name.StartsWith("Microsoft.", StringComparison.OrdinalIgnoreCase)).ToArray())
                .AddNaos(configuration, "Product", "Capability", new[] { "All" }, n => n
                    .AddOperations(o => o
                        .AddLogging(correlationId: $"TEST{RandomGenerator.GenerateString(9)}"))
                    .AddMessaging(o => o
                        //.AddFileSystemBroker()
                        //.AddSignalRBroker()
                        .UseServiceBusBroker()));

            this.ServiceProvider = this.Services.BuildServiceProvider();
            this.sut = this.ServiceProvider.GetService<IMessageBroker>();
        }

        [Fact]
        public void CanInstantiate_Test()
        {
            this.sut.ShouldNotBeNull();
        }

        [Fact]
        public void CanPublish_Test()
        {
            this.sut.Publish(new Core.Messaging.Domain.Message());

            this.sut.ShouldNotBeNull();
        }
    }
}
