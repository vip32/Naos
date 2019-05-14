namespace Naos.Core.Commands.Domain
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public class JournalCommandBehavior : ICommandBehavior
    {
        private readonly ILogger<JournalCommandBehavior> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="JournalCommandBehavior"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public JournalCommandBehavior(ILogger<JournalCommandBehavior> logger)
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

            this.logger.LogJournal(LogKeys.AppCommand, $"send (name={request.GetType().Name.SubstringTill("Command")}, id={request.Id}", LogEventPropertyKeys.TrackSendCommand);
            return await Task.FromResult(new CommandBehaviorResult()).AnyContext();
        }
    }
}
