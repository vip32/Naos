namespace Naos.Core.Messaging
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public class DummyMessageHandler : IMessageHandler<DummyMessage>
    {
        protected readonly ILogger<DummyMessageHandler> logger;

        public DummyMessageHandler(ILogger<DummyMessageHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The event.</param>
        /// <returns></returns>
        public virtual Task Handle(DummyMessage message)
        {
            using (this.logger.BeginScope("{CorrelationId}", message.CorrelationId))
            {
                this.logger.LogInformation("MESSAGE handle  (name={MessageName}, id={MessageId}, origin={MessageOrigin}) " + message.Data, message.GetType().PrettyName(), message.Id, message.Origin);

                return Task.CompletedTask;
            }
        }
    }
}