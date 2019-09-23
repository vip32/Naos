namespace Naos.Commands.App
{
    using System.Threading.Tasks;

    public interface ICommandBehavior
    {
        ICommandBehavior SetNext(ICommandBehavior next);

        /// <summary>
        /// Executes this behavior for the specified command.
        /// </summary>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request">The command request applied to this behavior.</param>
        /// <param name="result">The command behaviors result</param>
        Task ExecutePreHandleAsync<TResponse>(Command<TResponse> request, CommandBehaviorResult result);

        Task ExecutePostHandleAsync<TResponse>(CommandResponse<TResponse> response, CommandBehaviorResult result);
    }
}
