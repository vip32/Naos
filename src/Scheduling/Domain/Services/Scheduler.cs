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
        private readonly Dictionary<Registration, IScheduledTask> registrations = new Dictionary<Registration, IScheduledTask>();
        private readonly ILogger<Scheduler> logger;
        private readonly IMutex mutex;
        private readonly IScheduledTaskFactory taskFactory;
        private int activeCount;
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

        public bool IsRunning => this.activeCount > 0;

        public IScheduler Register(string cron, Action<string[]> action)
        {
            return this.Register(new Registration(null, cron), new ScheduledTask(action));
        }

        public IScheduler Register(string key, string cron, Action<string[]> action)
        {
            return this.Register(new Registration(key, cron), new ScheduledTask(action));
        }

        public IScheduler Register(string cron, Func<string[], Task> func)
        {
            return this.Register(new Registration(null, cron), new ScheduledTask(func));
        }

        public IScheduler Register(string key, string cron, Func<string[], Task> func)
        {
            return this.Register(new Registration(key, cron), new ScheduledTask(func));
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
                new Registration(key, cron),
                new ScheduledTask(async (a) => // defer task creation
                {
                    var task = this.taskFactory.Create(typeof(T));
                    if(task == null)
                    {
                        throw new NaosException($"Cannot create instance for type {typeof(T).PrettyName()}.");
                    }

                    await task.ExecuteAsync(a).ConfigureAwait(false);
                }));
        }

        public IScheduler Register(Registration registration, IScheduledTask task)
        {
            EnsureArg.IsNotNull(registration, nameof(registration));
            EnsureArg.IsNotNullOrEmpty(registration.Cron, nameof(registration.Cron));

            if (task != null)
            {
                registration.Key = registration.Key ?? HashAlgorithm.ComputeHash(task);
                this.logger.LogInformation($"register scheduled task (key={registration.Key}, cron={registration.Cron})");
                this.registrations.Add(registration, task); // TODO: remove existing by key
            }

            return this;
        }

        public IScheduler UnRegister(string key)
        {
            return this.UnRegister(this.GetRegistationByKey(key));
        }

        public IScheduler UnRegister(Registration registration)
        {
            if (registration != null)
            {
                this.registrations.Remove(registration);
            }

            return this;
        }

        public IScheduler OnError(Action<Exception> errorHandler)
        {
            this.errorHandler = errorHandler;
            return this;
        }

        public async Task TriggerAsync(string key)
        {
            var task = this.GetTaskByKey(key);
            if (task != null)
            {
                await this.ExecuteTaskAsync(key, task).ConfigureAwait(false);
            }
        }

        public async Task RunAsync()
        {
            await this.RunAsync(DateTime.UtcNow);
        }

        public async Task RunAsync(DateTime moment)
        {
            EnsureArg.IsTrue(moment.Kind == DateTimeKind.Utc);

            Interlocked.Increment(ref this.activeCount);
            this.logger.LogInformation($"scheduler run started (activeCount=#{this.activeCount}, moment={moment.ToString("o")})");
            await this.ExecuteTasksAsync(moment).ConfigureAwait(false);
            Interlocked.Decrement(ref this.activeCount);
            this.logger.LogInformation($"scheduler run finished (activeCount=#{this.activeCount})");
        }

private async Task ExecuteTasksAsync(DateTime moment)
        {
            var dueTasks = this.registrations.Where(t => t.Key?.IsDue(moment) == true).Select(t =>
            {
                return Task.Run(() =>
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    this.ExecuteTaskAsync(t.Key.Key, t.Value); // dont use await for better parallism
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                });
            }).ToList();

            if (dueTasks.IsNullOrEmpty())
            {
                this.logger.LogInformation("scheduler run found no due tasks");
            }

            await Task.WhenAll(dueTasks).ConfigureAwait(false); // really wait for completion (await)?
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
                    if (this.mutex.TryAcquireLock(key))
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

        private Registration GetRegistationByKey(string key)
        {
            return this.registrations.Where(r => r.Key.Key.SafeEquals(key)).Select(r => r.Key).FirstOrDefault();
        }

        private IScheduledTask GetTaskByKey(string key)
        {
            return this.registrations.Where(r => r.Key.Key.SafeEquals(key)).Select(r => r.Value).FirstOrDefault();
        }
    }
}
