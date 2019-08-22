namespace Naos.Core.Commands.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    /// <summary>
    /// Echo handler for the <see cref="TRequest" /> command request, response result is always the input message.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <seealso cref="App.BehaviorCommandHandler{TRequest, bool}" />
    public class EchoCommandHandler : BehaviorCommandHandler<EchoCommand, EchoCommandResponse>
    {
        private readonly ILogger<EchoCommandHandler> logger;

        public EchoCommandHandler(
            ILogger<EchoCommandHandler> logger,
            IEnumerable<ICommandBehavior> behaviors)
            : base(logger, behaviors)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public override async Task<CommandResponse<EchoCommandResponse>> HandleRequest(EchoCommand request, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                request.Properties.AddOrUpdate(this.GetType().Name, true);

                this.logger.LogInformation($"{{LogKey:l}} {request.GetType().Name} (handler={this.GetType().Name})", LogKeys.AppCommand);

                return new CommandResponse<EchoCommandResponse>
                {
                    Result = new EchoCommandResponse { Message = request.Message ?? "echo" }
                };
            }).AnyContext();
        }
    }
}
