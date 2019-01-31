namespace Naos.Core.Messaging
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class EntityMessageHandler<T> : IMessageHandler<EntityMessage<T>>
        where T : class, IEntity
    {
        protected readonly ILogger<EntityMessageHandler<T>> logger;

        public EntityMessageHandler(ILogger<EntityMessageHandler<T>> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public virtual Task Handle(EntityMessage<T> message)
        {
            var loggerState = new Dictionary<string, object>
            {
                [LogEventPropertyKeys.CorrelationId] = message.CorrelationId,
            };

            using (this.logger.BeginScope(loggerState))
            {
                this.logger.LogInformation("{LogKey:l} handle (name={MessageName}, id={MessageId}, origin={MessageOrigin}) " + message.Entity.GetType().Name, LogEventKeys.Messaging, message.GetType().PrettyName(), message.Id, message.Origin);

                return Task.CompletedTask;
            }
        }
    }
}