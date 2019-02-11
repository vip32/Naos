namespace Naos.Sample.App.IntegrationTests.Messaging.Infrastructure.Azure
{
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;
    using Naos.Core.Common;
    using Naos.Core.Configuration;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Domain.Model;
    using Shouldly;
    using Xunit;

    public class MessageBrokerTests : BaseTest
    {
        private readonly IServiceCollection services = new ServiceCollection();

        public MessageBrokerTests()
        {
            var configuration = NaosConfigurationFactory.Create();

            this.services
                .AddMediatR()
                .AddNaos(configuration, "Product", "Capability")
                    .AddOperationsSerilog(correlationId: $"TEST{RandomGenerator.GenerateString(9, true)}")
                    //.AddMessaging(o => o.AddFileSystemBroker());
                    //.AddMessaging(o => o.AddSignalRBroker());
                    .AddMessaging(o => o.AddServiceBusBroker());

            this.ServiceProvider = this.services.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; private set; }

        //[Fact]
        //public void VerifyContainer_Test()
        //{
        //    this.container.Verify();
        //}

        [Fact]
        public void CanInstantiate_Test()
        {
            var sut = this.ServiceProvider.GetService<IMessageBroker>();

            sut.ShouldNotBeNull();
        }

        [Fact]
        public void CanPublish_Test()
        {
            var sut = this.ServiceProvider.GetService<IMessageBroker>();
            sut.Publish(new Message());

            sut.ShouldNotBeNull();
        }
    }
}
