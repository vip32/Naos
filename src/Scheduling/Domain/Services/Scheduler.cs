namespace Naos.Core.Scheduling.Domain.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public class Scheduler : IScheduler
    {
        private readonly Dictionary<string, ScheduledTask> tasks = new Dictionary<string, ScheduledTask>();
        private readonly ILogger<Scheduler> logger;
        private readonly IScheduledTaskFactory taskFactory;
        private int activeCount = 0;
        private Action<Exception> errorHandler;

        // register tasks
        // run registered task (host calls based on timer)

        public Scheduler(ILogger<Scheduler> logger, IScheduledTaskFactory taskFactory)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.taskFactory = taskFactory;
        }

        public IScheduler Register(string cron, Action action)
        {
            return this.Register(new ScheduledTask(cron, action));
        }

        public IScheduler Register(string key, string cron, Action action)
        {
            return this.Register(key, new ScheduledTask(cron, action));
        }

        public IScheduler Register(string cron, Func<Task> task)
        {
            return this.Register(new ScheduledTask(cron, task));
        }

        public IScheduler Register(string key, string cron, Func<Task> task)
        {
            return this.Register(key, new ScheduledTask(cron, task));
        }

        public IScheduler Register<T>(string cron)
            where T : IScheduledTask
        {
            if (!typeof(IScheduledTask).IsAssignableFrom(typeof(T)))
            {
                throw new NaosException("Task type to register must implement IScheduledTask.");
            }

            return this.Register(new ScheduledTask(cron, typeof(T)));
        }

        public IScheduler Register<T>(string key, string cron)
            where T : IScheduledTask
        {
            if (!typeof(IScheduledTask).IsAssignableFrom(typeof(T)))
            {
                throw new NaosException("Task type to register must implement IScheduledTask.");
            }

            return this.Register(key, new ScheduledTask(cron, typeof(T)));
        }

        public IScheduler Register(ScheduledTask scheduledTask)
        {
            return this.Register(null, scheduledTask);
        }

        public IScheduler Register(string key, ScheduledTask scheduledTask)
        {
            if (scheduledTask != null)
            {
                key = key ?? HashAlgorithm.ComputeHash(scheduledTask);
                this.logger.LogInformation($"register scheduled task (key={key})");
                this.tasks.AddOrUpdate(key, scheduledTask);
            }

            return this;
        }

        public IScheduler UnRegister(string key)
        {
            if (!key.IsNullOrEmpty() && this.tasks.ContainsKey(key))
            {
                this.tasks.Remove(key);
            }

            return this;
        }

        public IScheduler OnError(Action<Exception> errorHandler)
        {
            this.errorHandler = errorHandler;
            return this;
        }

        public async Task RunAsync()
        {
            await this.RunAsync(DateTime.UtcNow);
        }

        public async Task RunAsync(DateTime fromUtc)
        {
            Interlocked.Increment(ref this.activeCount);
            await this.RunTasks(fromUtc);
            Interlocked.Decrement(ref this.activeCount);
        }

        private async Task RunTasks(DateTime fromUtc)
        {
            var activeTasks = this.tasks.Where(t => t.Value?.IsDue(fromUtc) == true).Select(t =>
            {
                return Task.Run(async () =>
                {
                    await this.ExecuteTask(t.Key, t.Value);
                });
            });

            await Task.WhenAll(activeTasks); // really wait for completion?
        }

        private async Task ExecuteTask(string key, ScheduledTask task)
        {
            try
            {
                // TODO: publish domain event (task started)
                this.logger.LogInformation($"scheduled task started (key={key})");

                async Task Execute()
                {
                    await task.ExecuteAsync();
                }

                await Execute();
                // TODO: overlap check here

                this.logger.LogInformation($"scheduled task finished (key={key})");
                // TODO: publish domain event (task finished)
            }
            catch (Exception ex)
            {
                // TODO: publish domain event (task failed)
                this.logger.LogError($"scheduled task failed (key={key})");

                this.errorHandler?.Invoke(ex);
            }
        }
    }
}
