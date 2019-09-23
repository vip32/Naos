namespace Naos.Commands.App
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    public class JournalCommandBehavior : ICommandBehavior
    {
        private readonly ILogger<JournalCommandBehavior> logger;
        private ICommandBehavior next;

        /// <summary>
        /// Initializes a new instance of the <see cref="JournalCommandBehavior"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public JournalCommandBehavior(ILogger<JournalCommandBehavior> logger)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
        }

        public ICommandBehavior SetNext(ICommandBehavior next)
        {
            this.next = next;
            return next;
        }

        /// <summary>
        /// Executes this behavior for the specified command.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The command.</param>
        public async Task ExecutePreHandleAsync<TResponse>(Command<TResponse> request, CommandBehaviorResult result)
        {
            EnsureArg.IsNotNull(request);

            this.logger.LogJournal(LogKeys.AppCommand, $"send (name={request.GetType().PrettyName()}, id={request.Id})", LogPropertyKeys.TrackSendCommand);

            if (!result.Cancelled && this.next != null)
            {
                await this.next.ExecutePreHandleAsync(request, result).AnyContext();
            }

            // terminate here
        }

        public async Task ExecutePostHandleAsync<TResponse>(CommandResponse<TResponse> response, CommandBehaviorResult result)
        {
            if (!result.Cancelled && this.next != null)
            {
                await this.next.ExecutePostHandleAsync(response, result).AnyContext();
            }
        }
    }
}
