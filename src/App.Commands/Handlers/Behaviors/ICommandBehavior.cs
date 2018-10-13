namespace Naos.Core.App.Commands
{
    using System.Threading.Tasks;

    public interface ICommandBehavior
    {
        /// <summary>
        /// Executes this behavior for the specified command
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command">The command applied to this behavior.</param>
        /// <returns></returns>
        Task<CommandBehaviorResult> ExecuteAsync<T>(CommandRequest<T> command);
    }
}
