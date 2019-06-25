namespace Naos.Core.JobScheduling.App
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Naos.Core.JobScheduling.Domain;
    using Naos.Foundation;

    public class JobSchedulerHostedService : IHostedService, IDisposable // TODO: or use BackgroundService? https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/multi-container-microservice-net-applications/background-tasks-with-ihostedservice#implementing-ihostedservice-with-a-custom-hosted-service-class-deriving-from-the-backgroundservice-base-class
    {
        private readonly ILogger<JobSchedulerHostedService> logger;
        private readonly IJobScheduler scheduler;
        private bool enabled = true; // TODO: start/stop from outside https://stackoverflow.com/questions/51469881/asp-net-core-ihostedservice-manual-start-stop-pause
        private System.Threading.Timer schedulerTimer;

        public JobSchedulerHostedService(
            ILoggerFactory loggerFactory,
            IJobScheduler scheduler)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(scheduler, nameof(scheduler));

            this.logger = loggerFactory.CreateLogger<JobSchedulerHostedService>();
            this.scheduler = scheduler;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if(!cancellationToken.IsCancellationRequested)
            {
                var moment = DateTime.UtcNow;
                this.schedulerTimer = new System.Threading.Timer(
                    this.RunSchedulerAsync,
                    null,
                    new DateTime(moment.Year, moment.Month, moment.Day, moment.Hour, moment.Minute, 59, 999, DateTimeKind.Utc) - moment, // trigger on the minute start
                    TimeSpan.FromMinutes(1));
                this.logger.LogInformation($"{{LogKey:l}} hosted service started (moment={moment.ToString("o")})", LogKeys.JobScheduling);
            }

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogInformation("{LogKey:l} hosted service stopping", LogKeys.JobScheduling);
            this.enabled = false;
            this.schedulerTimer?.Change(Timeout.Infinite, 0);

            // suspend stopping schedular untill all tasks are done
            if(this.scheduler.IsRunning)
            {
                this.logger.LogWarning("{LogKey:l} hosted service will be stopped but is waiting on running jobs", LogKeys.JobScheduling);
            }

            while(this.scheduler.IsRunning && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(50); // TODO: try to cancel the running jobs
            }
        }

        public void Dispose()
        {
            this.schedulerTimer?.Dispose();
            this.logger.LogInformation("{LogKey:l} hosted service stopped", LogKeys.JobScheduling);
        }

        private async void RunSchedulerAsync(object state)
        {
            if(this.enabled)
            {
                await this.scheduler.RunAsync().AnyContext();
            }
        }
    }
}
