namespace Naos.Core.Commands.App
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

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
        public async Task<CommandBehaviorResult> ExecuteAsync<TResponse>(Command<TResponse> request)
        {
            EnsureArg.IsNotNull(request);

            this.logger.LogJournal(LogKeys.AppCommand, $"send (name={request.GetType().PrettyName()}, id={request.Id})", LogPropertyKeys.TrackSendCommand);
            return await Task.FromResult(new CommandBehaviorResult()).AnyContext();
        }
    }
}
