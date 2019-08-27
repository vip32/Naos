namespace Naos.Core.Messaging.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    public class DummyMessageHandler : IMessageHandler<DummyMessage>
    {
        public DummyMessageHandler(ILogger<DummyMessageHandler> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.Logger = logger;
        }

        protected ILogger<DummyMessageHandler> Logger { get; }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The event.</param>
        public virtual Task Handle(DummyMessage message)
        {
            var loggerState = new Dictionary<string, object>
            {
                [LogPropertyKeys.CorrelationId] = message.CorrelationId,
            };

            using (this.Logger.BeginScope(loggerState))
            {
                this.Logger.LogInformation("{LogKey:l} handle (name={MessageName}, id={MessageId}, origin={MessageOrigin}) " + message.Data, LogKeys.Messaging, message.GetType().PrettyName(), message.Id, message.Origin);

                return Task.CompletedTask;
            }
        }
    }
}