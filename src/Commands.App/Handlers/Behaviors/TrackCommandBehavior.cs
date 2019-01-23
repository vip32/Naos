namespace Naos.Core.Commands.App
{
    using System.Collections.Generic;
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

            var loggerState = new Dictionary<string, object>
            {
                [LogEventPropertyKeys.TrackType] = LogEventTrackTypeValues.Journal,
                [LogEventPropertyKeys.TrackCommand] = true
            };

            using (this.logger.BeginScope(loggerState))
            {
                this.logger.LogInformation($"{{LogKey}} {command.GetType().Name.SubstringTill("Command")}", LogEventKeys.AppCommand);
            }

            return await Task.FromResult(new CommandBehaviorResult()).ConfigureAwait(false);
        }
    }
}
