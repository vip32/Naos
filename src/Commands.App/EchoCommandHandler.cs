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
    /// Echo handler for the <see cref="TRequest" /> command request, response result is always the input message.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <seealso cref="App.BaseCommandHandler{TRequest, bool}" />
    public class EchoCommandHandler : BaseCommandHandler<EchoCommand, EchoCommandResponse>
    {
        private readonly ILogger<EchoCommandHandler> logger;

        public EchoCommandHandler(
            ILogger<EchoCommandHandler> logger,
            ITracer tracer = null,
            IEnumerable<ICommandBehavior> behaviors = null)
            : base(logger, tracer, behaviors)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public override async Task<CommandResponse<EchoCommandResponse>> HandleRequest(EchoCommand request, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                request.Properties.AddOrUpdate(this.GetType().Name, true);
                var message = $"+++ {request.Message ?? "echo"}";

                this.logger.LogInformation($"{{LogKey:l}} {message} (handler={this.GetType().Name})", LogKeys.AppCommand);

                return new CommandResponse<EchoCommandResponse>
                {
                    Result = new EchoCommandResponse { Message = message, /*Source = request*/ }
                };
            }).AnyContext();
        }
    }
}
