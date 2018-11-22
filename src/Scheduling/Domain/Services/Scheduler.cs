namespace Naos.Core.Scheduling.Domain
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
        private readonly Dictionary<string, IScheduledTask> tasks = new Dictionary<string, IScheduledTask>();
        private readonly ILogger<Scheduler> logger;
        private readonly IMutex mutex;
        private readonly IScheduledTaskFactory taskFactory;
        private int activeCount = 0;
        private Action<Exception> errorHandler;

        // register tasks
        // run registered task (host calls based on timer)

        public Scheduler(ILogger<Scheduler> logger, IScheduledTaskFactory taskFactory, IMutex mutex)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.mutex = mutex ?? new InMemoryMutex();
            this.taskFactory = taskFactory; // what to do when null?
        }

        public IScheduler Register(string cron, Action<string[]> action)
        {
            return this.Register(new ScheduledTask(cron, action));
        }

        public IScheduler Register(string key, string cron, Action<string[]> action)
        {
            return this.Register(key, new ScheduledTask(cron, action));
        }

        public IScheduler Register(string cron, Func<string[], Task> task)
        {
            return this.Register(new ScheduledTask(cron, task));
        }

        public IScheduler Register(string key, string cron, Func<string[], Task> task)
        {
            return this.Register(key, new ScheduledTask(cron, task));
        }

        public IScheduler Register<T>(string cron, string[] args = null)
            where T : IScheduledTask
        {
            return this.Register<T>(null, cron);
        }

        public IScheduler Register<T>(string key, string cron, string[] args = null)
            where T : IScheduledTask
        {
            if (!typeof(ScheduledTask).IsAssignableFrom(typeof(T)))
            {
                throw new NaosException("Task type to register must implement IScheduledTask.");
            }

            return this.Register(
                key,
                new ScheduledTask(cron, async (a) => // defer task creation
                {
                    var task = this.taskFactory.Create(typeof(T));
                    if(task == null)
                    {
                        throw new NaosException($"Cannot create instance for type {typeof(T).PrettyName()}.");
                    }

                    await task.ExecuteAsync(a).ConfigureAwait(false);
                }));
        }

        public IScheduler Register(IScheduledTask scheduledTask)
        {
            return this.Register(null, scheduledTask);
        }

        public IScheduler Register(string key, IScheduledTask scheduledTask)
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

        public async Task TriggerAsync(string key)
        {
            var task = this.tasks.FirstOrDefault(t => t.Key.SafeEquals(key));
            if (task.Value != null)
            {
                await this.ExecuteTaskAsync(key, task.Value).ConfigureAwait(false);
            }
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
            await this.ExecuteTasksAsync(fromUtc).ConfigureAwait(false);
            Interlocked.Decrement(ref this.activeCount);
        }

        private async Task ExecuteTasksAsync(DateTime fromUtc)
        {
            var activeTasks = this.tasks.Where(t => t.Value?.IsDue(fromUtc) == true).Select(t =>
            {
                return Task.Run(() =>
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.ExecuteTaskAsync(t.Key, t.Value); // dont use await for better parallism
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                });
            });

            await Task.WhenAll(activeTasks).ConfigureAwait(false); // really wait for completion?
        }

        private async Task ExecuteTaskAsync(string key, IScheduledTask task)
        {
            if (!key.IsNullOrEmpty() && task != null)
            {
                try
                {
                    async Task Execute()
                    {
                        // TODO: publish domain event (task started)
                        this.logger.LogInformation($"scheduled task started (key={key}, type={task.GetType().PrettyName()})");
                        await task.ExecuteAsync().ConfigureAwait(false);
                        this.logger.LogInformation($"scheduled task finished (key={key}, type={task.GetType().PrettyName()})");
                        // TODO: publish domain event (task finished)
                    }

                    //if (task.PreventOverlap)
                    //{
                    if (this.mutex.TryGetLock(key, 1440))
                    {
                        try
                        {
                            await Execute();
                        }
                        finally
                        {
                            this.mutex.ReleaseLock(key);
                        }
                    }
                    else
                    {
                        this.logger.LogWarning($"scheduled task already executing (key={key}, type={task.GetType().PrettyName()})");
                    }

                    //}
                }
                catch (Exception ex)
                {
                    // TODO: publish domain event (task failed)
                    this.logger.LogError($"scheduled task failed (key={key}), type={task.GetType().PrettyName()})");

                    this.errorHandler?.Invoke(ex);
                }
            }
        }
    }
}
