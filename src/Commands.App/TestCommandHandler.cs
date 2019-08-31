namespace Naos.Core.Commands.App
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    /// <summary>
    /// Test handler for the <see cref="TRequest" /> command request, response result is always true.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <seealso cref="App.BehaviorCommandHandler{TRequest, bool}" />
    public class TestCommandHandler<TRequest> : BehaviorCommandHandler<TRequest, bool>
        where TRequest : Command<bool>
    {
        public TestCommandHandler(
            ILogger<TestCommandHandler<TRequest>> logger,
            IEnumerable<ICommandBehavior> behaviors)
            : base(logger, behaviors)
        {
        }

        /// <summary>
        /// Handles the command. Response will always have a true result.
        /// </summary>
        /// <param name="request">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public override async Task<CommandResponse<bool>> HandleRequest(TRequest request, CancellationToken cancellationToken)
        {
            this.Logger.LogJournal(LogKeys.AppCommand, $"[{request.Identifier}] handle {typeof(TRequest).Name.SliceTill("Command")}", LogPropertyKeys.TrackHandleCommand);

            return await Task.FromResult(new CommandResponse<bool>
            {
                Result = true
            }).AnyContext();
        }
    }
}
