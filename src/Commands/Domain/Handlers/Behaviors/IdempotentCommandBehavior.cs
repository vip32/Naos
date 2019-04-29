namespace Naos.Core.Commands.Domain
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Common;

    public class IdempotentCommandBehavior : ICommandBehavior
    {
        /// <summary>
        /// Executes this behavior for the specified command.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The command.</param>
        /// <returns></returns>
        public async Task<CommandBehaviorResult> ExecuteAsync<TResponse>(CommandRequest<TResponse> request)
        {
            EnsureArg.IsNotNull(request);

            // TODO: implement
            // - check if command exists in repo
            // - if so return CommandBehaviorResult cancelled = true + reason

            return await Task.FromResult(new CommandBehaviorResult()).AnyContext();
            //return await Task.FromResult(new BehaviorResult("command already handled")).AnyContext();
        }
    }
}
