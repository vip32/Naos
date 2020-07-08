namespace Naos.Foundation.Application
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.AspNetCore.Hosting.Server;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    /// <summary>
    /// A server that executes <see cref="IStartupTask"/>s on startup,
    /// and then invokes a wrapped <see cref="IServer"/> instance
    /// </summary>
    public class StartupTaskServerDecorator : IServer
    {
        private readonly IEnumerable<IStartupTask> tasks;
        private readonly IServer inner;
        private readonly ILogger<StartupTaskServerDecorator> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartupTaskServerDecorator"/> class.
        /// </summary>
        /// <param name="tasks">The tasks to execute on startup</param>
        /// <param name="inner">The decorated server instance </param>
        public StartupTaskServerDecorator(ILoggerFactory loggerFactory, IEnumerable<IStartupTask> tasks, IServer inner)
        {
            EnsureArg.IsNotNull(inner, nameof(inner));

            this.tasks = tasks.Safe();
            this.logger = loggerFactory.CreateLogger<StartupTaskServerDecorator>();
            this.inner = inner;
        }

        /// <inheritdoc />
        public IFeatureCollection Features => this.inner.Features;

        /// <inheritdoc />
        public async Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        {
            foreach (var task in this.tasks)
            {
                if (task.Delay.HasValue && task.Delay.Value > TimeSpan.Zero)
                {
                    _ = Run.DelayedAsync(task.Delay.Value, async () =>
                      {
                          this.logger.LogInformation($"{{LogKey:l}} task started: {task.GetType().PrettyName()} (delay={task.Delay.Value})", LogKeys.StartupTask);
                          await task.StartAsync(cancellationToken).AnyContext();
                      });
                }
                else
                {
                    this.logger.LogInformation($"{{LogKey:l}} task started: {task.GetType().PrettyName()}", LogKeys.StartupTask);
                    await task.StartAsync(cancellationToken).AnyContext();
                }
            }

            await this.inner.StartAsync(application, cancellationToken).AnyContext();
        }

        /// <inheritdoc />
        public void Dispose() => this.inner.Dispose();

        /// <inheritdoc />
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await this.inner.StopAsync(cancellationToken).AnyContext();

            foreach (var task in this.tasks)
            {
                this.logger.LogInformation($"{{LogKey:l}} task stopped: {task.GetType().PrettyName()}", LogKeys.StartupTask);
                await task.ShutdownAsync(cancellationToken).AnyContext();
            }
        }
    }
}
