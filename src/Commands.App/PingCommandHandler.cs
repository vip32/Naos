namespace Naos.Commands.App
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Tracing.Domain;

    /// <summary>
    /// Ping handler for the <see cref="TRequest" /> command request, response has no result.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <seealso cref="App.BaseCommandHandler{TRequest, bool}" />
    public class PingCommandHandler : BaseCommandHandler<PingCommand, object>
    {
        private readonly ILogger<EchoCommandHandler> logger;

        public PingCommandHandler(
            ILogger<EchoCommandHandler> logger,
            ITracer tracer = null,
            IEnumerable<ICommandBehavior> behaviors = null)
            : base(logger, tracer, behaviors)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public override async Task<CommandResponse<object>> HandleRequest(PingCommand request, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                request.Properties.AddOrUpdate(this.GetType().Name, true);

                this.logger.LogInformation($"{{LogKey:l}} +++ echo {request.GetType().Name}", LogKeys.AppCommand);

                return new CommandResponse<object>(); // no result
            }).AnyContext();
        }
    }
}
