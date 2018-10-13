namespace Naos.Core.Messaging
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;

    public class TestMessageHandler : IMessageHandler<TestMessage>
    {
        protected readonly ILogger<TestMessageHandler> logger;

        public TestMessageHandler(ILogger<TestMessageHandler> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The event.</param>
        /// <returns></returns>
        public virtual Task Handle(TestMessage message)
        {
            //using (LogContext.PushProperty("CorrelationId", message.CorrelationId))
            //{
                this.logger.LogInformation("handle  message (id={MessageId}, origin={MessageOrigin}) " + message.Data, message.Id, message.Origin);

                return Task.CompletedTask;
            //}
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class DummyMessageHandler : IMessageHandler<DummyMessage>
#pragma warning restore SA1402 // File may only contain a single class
    {
        protected readonly ILogger<DummyMessageHandler> logger;

        public DummyMessageHandler(ILogger<DummyMessageHandler> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The event.</param>
        /// <returns></returns>
        public virtual Task Handle(DummyMessage message)
        {
            //using (LogContext.PushProperty("CorrelationId", message.CorrelationId))
            //{
                this.logger.LogInformation("handle  message (id={MessageId}, origin={MessageOrigin}) " + message.Data, message.Id, message.Origin);

                return Task.CompletedTask;
            //}
        }
    }
}