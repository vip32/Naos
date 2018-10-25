namespace Naos.Core.Sample.Messaging.App.Console
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Messaging;

    public class StubEntityMessageHandler : EntityMessageHandler<StubEntity>
    {
        public StubEntityMessageHandler(ILogger<StubEntityMessageHandler> logger)
            : base(logger)
        {
        }

        public override Task Handle(EntityMessage<StubEntity> message)
        {
            using (this.logger.BeginScope("{CorrelationId}", message.CorrelationId))
            {
                this.logger.LogInformation("handle  message (name={MessageName}, id={EventId}, origin={EventOrigin}) " + $"{message.Entity.FirstName} {message.Entity.LastName}", message.GetType().PrettyName(), message.Id, message.Origin);

                return Task.CompletedTask;
            }
        }
    }
}
