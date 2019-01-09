namespace Naos.Core.Messaging
{
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
            using (this.logger.BeginScope("{CorrelationId}", message.CorrelationId))
            {
                this.logger.LogInformation("MESSAGE handle  (name={MessageName}, id={MessageId}, origin={MessageOrigin}) " + message.Entity.GetType().Name, message.GetType().PrettyName(), message.Id, message.Origin);

                return Task.CompletedTask;
            }
        }
    }
}