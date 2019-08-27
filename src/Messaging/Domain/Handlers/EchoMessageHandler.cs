namespace Naos.Core.Messaging.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    public class EchoMessageHandler : IMessageHandler<EchoMessage>
    {
        public EchoMessageHandler(ILogger<EchoMessageHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.Logger = logger;
        }

        protected ILogger<EchoMessageHandler> Logger { get; }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The event.</param>
        public virtual Task Handle(EchoMessage message)
        {
            var loggerState = new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = message.CorrelationId,
            };

            using (this.Logger.BeginScope(loggerState))
            {
                this.Logger.LogInformation($"{{LogKey:l}} {message.Text} (name={{MessageName}}, id={{MessageId}}, origin={{MessageOrigin}}) ", LogKeys.Messaging, message.GetType().PrettyName(), message.Id, message.Origin);
                Thread.Sleep(1500);
                return Task.CompletedTask;
            }
        }
    }
}