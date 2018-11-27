namespace Naos.Core.Scheduling.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Scheduling.Domain;
    using SimpleInjector;

    public class JobSchedulerHostedService : IHostedService, IDisposable
    {
        private readonly ILogger<JobSchedulerHostedService> logger;
        private readonly IJobScheduler scheduler;
        private bool enabled = true; // TODO: start/stop from outside https://stackoverflow.com/questions/51469881/asp-net-core-ihostedservice-manual-start-stop-pause
        private Timer schedulerTimer;

        public JobSchedulerHostedService(ILogger<JobSchedulerHostedService> logger, Container container)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(container, nameof(container));

            this.logger = logger;
            this.scheduler = container.GetInstance<IJobScheduler>();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var moment = DateTime.UtcNow;
            this.schedulerTimer = new Timer(
                this.RunSchedulerAsync,
                null,
                new DateTime(moment.Year, moment.Month, moment.Day, moment.Hour, moment.Minute, 59, 999, DateTimeKind.Utc) - moment, // trigger on the minute start
                TimeSpan.FromMinutes(1));
            this.logger.LogInformation($"scheduler hosted service started (moment={moment.ToString("o")})");
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("scheduler hosted service stopping");
            this.enabled = false;
            this.schedulerTimer?.Change(Timeout.Infinite, 0);

            // suspend stopping schedular untill all tasks are done
            if (this.scheduler.IsRunning)
            {
                this.logger.LogWarning("scheduler hosted service will be stopped but is waiting on running jobs");
            }

            while (this.scheduler.IsRunning)
            {
                await Task.Delay(50); // TODO: try to cancel the running jobs
            }
        }

        public void Dispose()
        {
            this.schedulerTimer?.Dispose();
            this.logger.LogInformation("scheduler hosted service stopped");
        }

        private async void RunSchedulerAsync(object state)
        {
            if (this.enabled)
            {
                await this.scheduler.RunAsync().ConfigureAwait(false);
            }
        }
    }
}
