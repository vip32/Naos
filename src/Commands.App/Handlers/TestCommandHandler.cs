namespace Naos.Core.Commands.App
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    /// <summary>
    /// Test handler for the <see cref="TRequest" /> command request, response result is always true
    /// </summary>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <seealso cref="App.BehaviorCommandHandler{TRequest, bool}" />
    public class TestCommandHandler<TRequest> : BehaviorCommandHandler<TRequest, bool>
        where TRequest : CommandRequest<bool>
    {
        public TestCommandHandler(IMediator mediator, IEnumerable<ICommandBehavior> behaviors)
            : base(mediator, behaviors)
        {
        }

        /// <summary>
        /// Handles the command. Response will always have a true result
        /// </summary>
        /// <param name="request">The command.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        public override async Task<CommandResponse<bool>> HandleRequest(TRequest request, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new CommandResponse<bool>
            {
                Result = true
            }).ConfigureAwait(false);
        }
    }
}
