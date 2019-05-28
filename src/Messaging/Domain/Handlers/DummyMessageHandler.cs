namespace Naos.Core.Messaging.Domain
{
    using System.Collections.Generic;
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
        public virtual Task Handle(DummyMessage message)
        {
            var loggerState = new Dictionary<string, object>
            {
                [LogEventPropertyKeys.CorrelationId] = message.CorrelationId,
            };

            using(this.logger.BeginScope(loggerState))
            {
                this.logger.LogInformation("{LogKey:l} handle (name={MessageName}, id={MessageId}, origin={MessageOrigin}) " + message.Data, LogKeys.Messaging, message.GetType().PrettyName(), message.Id, message.Origin);

                return Task.CompletedTask;
            }
        }
    }
}