namespace Naos.Core.Sample.Messaging.App.Console
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Messaging;
    using Serilog.Context;

    public class StubEntityMessageHandler : EntityMessageHandler<StubEntity>
    {
        public StubEntityMessageHandler(ILogger<StubEntityMessageHandler> logger)
            : base(logger)
        {
        }

        public override Task Handle(EntityMessage<StubEntity> @event)
        {
            using (LogContext.PushProperty("CorrelationId", @event.CorrelationId))
            {
                this.logger.LogInformation("handle  message (id={EventId}, origin={EventOrigin}) " + $"{@event.Entity.FirstName} {@event.Entity.LastName}", @event.Id, @event.Origin);

                return Task.CompletedTask;
            }
        }
    }
}
