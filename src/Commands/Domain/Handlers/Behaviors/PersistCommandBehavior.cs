namespace Naos.Core.Commands.Domain
{
    using System.Threading.Tasks;
    using EnsureThat;

    public class PersistCommandBehavior : ICommandBehavior
    {
        /// <summary>
        /// Executes this behavior for the specified command
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The command.</param>
        /// <returns></returns>
        public async Task<CommandBehaviorResult> ExecuteAsync<TResponse>(CommandRequest<TResponse> request)
        {
            EnsureArg.IsNotNull(request);
            // TODO: implement
            // - check if command exists in repo
            // - if not add to repo, return CommandBehaviorResult

            return await Task.FromResult(new CommandBehaviorResult()).ConfigureAwait(false);
        }
    }
}
