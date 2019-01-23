namespace Naos.Core.Messaging
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
        /// <returns></returns>
        public virtual Task Handle(DummyMessage message)
        {
            var loggerState = new Dictionary<string, object>
            {
                [LogEventPropertyKeys.CorrelationId] = message.CorrelationId,
            };

            using (this.logger.BeginScope(loggerState))
            {
                this.logger.LogInformation("{LogKey} handle (name={MessageName}, id={MessageId}, origin={MessageOrigin}) " + message.Data, LogEventKeys.Messaging, message.GetType().PrettyName(), message.Id, message.Origin);

                return Task.CompletedTask;
            }
        }
    }
}