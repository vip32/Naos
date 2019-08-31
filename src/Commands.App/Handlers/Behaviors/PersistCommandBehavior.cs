namespace Naos.Core.Commands.App
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Foundation;

    public class PersistCommandBehavior : ICommandBehavior
    {
        /// <summary>
        /// Executes this behavior for the specified command.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The command.</param>
        public async Task<CommandBehaviorResult> ExecuteAsync<TResponse>(Command<TResponse> request)
        {
            EnsureArg.IsNotNull(request);
            // TODO: implement
            // - check if command exists in repo/filestorage
            // - if not add to repo, return CommandBehaviorResult

            return await Task.FromResult(new CommandBehaviorResult()).AnyContext();
        }
    }
}
