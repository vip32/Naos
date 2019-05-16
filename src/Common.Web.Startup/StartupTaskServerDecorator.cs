namespace Naos.Core.Common.Web
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Http.Features;

    /// <summary>
    /// A server that executes <see cref="IStartupTask"/>s on startup,
    /// and then invokes a wrapped <see cref="IServer"/> instance
    /// </summary>
    public class StartupTaskServerDecorator : IServer
    {
        private readonly IEnumerable<IStartupTask> tasks;
        private readonly IServer decoratee;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupTaskServerDecorator"/> class.
        /// </summary>
        /// <param name="tasks">The tasks to execute on startup</param>
        /// <param name="decoratee">The decorated server instance </param>
        public StartupTaskServerDecorator(IEnumerable<IStartupTask> tasks, IServer decoratee)
        {
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.tasks = tasks.Safe();
            this.decoratee = decoratee;
        }

        /// <inheritdoc />
        public IFeatureCollection Features => this.decoratee.Features;

        /// <inheritdoc />
        public async Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            foreach(var task in this.tasks)
            {
                await task.StartAsync(cancellationToken).AnyContext();
            }

            await this.decoratee.StartAsync(application, cancellationToken).AnyContext();
        }

        /// <inheritdoc />
        public void Dispose() => this.decoratee.Dispose();

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this.decoratee.StopAsync(cancellationToken).AnyContext();

            foreach(var task in this.tasks)
            {
                await task.ShutdownAsync(cancellationToken).AnyContext();
            }
        }
    }
}
