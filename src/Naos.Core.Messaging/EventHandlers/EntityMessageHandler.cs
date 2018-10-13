namespace Naos.Core.Messaging
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Domain;

    public class EntityMessageHandler<TEntity> : IMessageHandler<EntityMessage<TEntity>>
        where TEntity : Entity<string>
    {
        protected readonly ILogger<EntityMessageHandler<TEntity>> logger;

        public EntityMessageHandler(ILogger<EntityMessageHandler<TEntity>> logger)
        {
            this.logger = logger;
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public virtual Task Handle(EntityMessage<TEntity> message)
        {
            //using (LogContext.PushProperty("CorrelationId", @event.CorrelationId))
            //{
                this.logger.LogInformation("handle  message (id={MessageId}, origin={MessageOrigin}) " + message.Entity.GetType().Name, message.Id, message.Origin);

                return Task.CompletedTask;
            //}
        }
    }
}