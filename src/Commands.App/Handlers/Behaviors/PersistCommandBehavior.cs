namespace Naos.Core.Commands.App
{
    using System.Threading.Tasks;
    using EnsureThat;

    public class PersistCommandBehavior : ICommandBehavior
    {
        /// <summary>
        /// Executes this behavior for the specified command
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="command">The command.</param>
        /// <returns></returns>
        public async Task<CommandBehaviorResult> ExecuteAsync<TResponse>(CommandRequest<TResponse> command)
        {
            EnsureArg.IsNotNull(command);
            // TODO: implement
            // - check if command exists in repo
            // - if not add to repo, return CommandBehaviorResult

            return await Task.FromResult(new CommandBehaviorResult()).ConfigureAwait(false);
        }
    }
}
