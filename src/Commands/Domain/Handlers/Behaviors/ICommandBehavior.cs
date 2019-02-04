namespace Naos.Core.Commands.Domain
{
    using System.Threading.Tasks;

    public interface ICommandBehavior
    {
        /// <summary>
        /// Executes this behavior for the specified command
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request">The command request applied to this behavior.</param>
        /// <returns></returns>
        Task<CommandBehaviorResult> ExecuteAsync<TResponse>(CommandRequest<TResponse> request);
    }
}
