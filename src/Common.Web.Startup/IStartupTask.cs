namespace Naos.Core.Common.Web
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A task to execute before starting the application
    /// </summary>
    public interface IStartupTask
    {
        /// <summary>
        /// Execute the startup task, before the WebHost is run
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for cancelling the task</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute the shutdown task, after the WebHost has stopped
        /// </summary>
        /// <param name="cancellationToken">A cancellation token for cancelling the task</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task ShutdownAsync(CancellationToken cancellationToken = default);
    }
}
