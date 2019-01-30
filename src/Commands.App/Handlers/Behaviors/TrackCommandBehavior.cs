namespace Naos.Core.Commands.App
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
        /// Executes this behavior for the specified command
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public async Task<CommandBehaviorResult> ExecuteAsync<TResponse>(CommandRequest<TResponse> command)
        {
            EnsureArg.IsNotNull(command);

            this.logger.LogJournal(LogEventPropertyKeys.TrackSendCommand, $"{{LogKey:l}} send {command.GetType().Name.SubstringTill("Command")}", args: LogEventKeys.AppCommand);
            return await Task.FromResult(new CommandBehaviorResult()).ConfigureAwait(false);
        }
    }
}
