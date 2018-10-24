namespace Naos.Sample.App.IntegrationTests.Messaging.Infrastructure.Azure
{
    using System;
    using Naos.Core.App.Configuration;
    using Naos.Core.App.Operations.Serilog;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Domain.Model;
    using Naos.Core.Messaging.Infrastructure.Azure;
    using SimpleInjector;
    using Xunit;

    public class ServiceBusMessageBusTests : BaseTest
    {
        private readonly Container container = new Container();

        public ServiceBusMessageBusTests()
        {
            this.container = new Container()
                .BuildNaosOperations()
                .BuildNaosMessaging(
                    NaosConfigurationFactory.CreateRoot(),
                    AppDomain.CurrentDomain.FriendlyName,
                    assemblies: new[] { typeof(IMessageBus).Assembly, typeof(ServiceBusMessageBusTests).Assembly });
            //this.container.Verify();
        }

        [Fact]
        public void VerifyContainer()
        {
            this.container.Verify();
        }

        [Fact]
        public void CanInstantiateMessageBus()
        {
            var messageBus = this.container.GetInstance<IMessageBus>();

            Assert.NotNull(messageBus);
        }

        [Fact]
        public void CanPublishMessageToMessageBus()
        {
            var messageBus = this.container.GetInstance<IMessageBus>();
            messageBus.Publish(new Message());

            Assert.NotNull(messageBus);
        }
    }
}
