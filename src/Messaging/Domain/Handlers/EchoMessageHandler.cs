namespace Naos.Core.Messaging.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public class EchoMessageHandler : IMessageHandler<EchoMessage>
    {
        protected readonly ILogger<EchoMessageHandler> logger;

        public EchoMessageHandler(ILogger<EchoMessageHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The event.</param>
        /// <returns></returns>
        public virtual Task Handle(EchoMessage message)
        {
            var loggerState = new Dictionary<string, object>
            {
                [LogEventPropertyKeys.CorrelationId] = message.CorrelationId,
            };

            using(this.logger.BeginScope(loggerState))
            {
                this.logger.LogInformation($"{{LogKey:l}} {message.Text} (name={{MessageName}}, id={{MessageId}}, origin={{MessageOrigin}}) ", LogKeys.Messaging, message.GetType().PrettyName(), message.Id, message.Origin);

                return Task.CompletedTask;
            }
        }
    }
}