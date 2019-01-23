namespace Naos.Core.Commands.App
{
    using System.Threading.Tasks;

    public interface ICommandBehavior
    {
        /// <summary>
        /// Executes this behavior for the specified command
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="command">The command applied to this behavior.</param>
        /// <returns></returns>
        Task<CommandBehaviorResult> ExecuteAsync<TResponse>(CommandRequest<TResponse> command);
    }
}
