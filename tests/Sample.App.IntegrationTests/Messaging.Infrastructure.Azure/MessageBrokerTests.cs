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

    public class MessageBrokerTests : BaseTest
    {
        private readonly Container container = new Container();

        public MessageBrokerTests()
        {
            var configuration = NaosConfigurationFactory.CreateRoot();
            this.container = new Container()
                .AddNaosLogging(configuration)
                .AddNaosMessaging(
                    configuration,
                    AppDomain.CurrentDomain.FriendlyName,
                    assemblies: new[] { typeof(IMessageBroker).Assembly, typeof(MessageBrokerTests).Assembly });
            this.container.Verify();
        }

        [Fact]
        public void VerifyContainer()
        {
            this.container.Verify();
        }

        [Fact]
        public void CanInstantiateMessageBroker()
        {
            var sut = this.container.GetInstance<IMessageBroker>();

            Assert.NotNull(sut);
        }

        [Fact]
        public void CanPublishToMessageBroker()
        {
            var sut = this.container.GetInstance<IMessageBroker>();
            sut.Publish(new Message());

            Assert.NotNull(sut);
        }
    }
}
