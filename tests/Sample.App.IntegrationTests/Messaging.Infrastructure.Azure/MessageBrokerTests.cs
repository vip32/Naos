namespace Naos.Sample.App.IntegrationTests.Messaging.Infrastructure.Azure
{
    using System;
    using Naos.Core.App.Configuration;
    using Naos.Core.App.Operations.Serilog;
    using Naos.Core.Messaging;
    using Naos.Core.Messaging.Domain.Model;
    using Naos.Core.Messaging.Infrastructure.Azure;
    using Shouldly;
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
        }

        [Fact]
        public void VerifyContainer_Test()
        {
            this.container.Verify();
        }

        [Fact]
        public void CanInstantiate_Test()
        {
            var sut = this.container.GetInstance<IMessageBroker>();

            sut.ShouldNotBeNull();
        }

        [Fact]
        public void CanPublish_Test()
        {
            var sut = this.container.GetInstance<IMessageBroker>();
            sut.Publish(new Message());

            sut.ShouldNotBeNull();
        }
    }
}
