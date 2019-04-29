namespace Naos.Core.Commands.Domain
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public class TrackCommandBehavior : ICommandBehavior
    {
        private readonly ILogger<TrackCommandBehavior> logger;

        public TrackCommandBehavior(ILogger<TrackCommandBehavior> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        /// <summary>
        /// Executes this behavior for the specified command.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The command.</param>
        /// <returns></returns>
        public async Task<CommandBehaviorResult> ExecuteAsync<TResponse>(CommandRequest<TResponse> request)
        {
            EnsureArg.IsNotNull(request);

            this.logger.LogJournal(LogKeys.AppCommand, $"[{request.Identifier}] send {request.GetType().Name.SubstringTill("Command")}", LogEventPropertyKeys.TrackSendCommand);
            return await Task.FromResult(new CommandBehaviorResult()).AnyContext();
        }
    }
}
