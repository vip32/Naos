namespace Naos.Core.Scheduling.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public class JobScheduler : IJobScheduler
    {
        private readonly Dictionary<JobRegistration, IJob> registrations = new Dictionary<JobRegistration, IJob>();
        private readonly ILogger<JobScheduler> logger;
        private readonly IMutex mutex;
        private readonly IJobFactory jobFactory;
        private int activeCount;
        private Action<Exception> errorHandler;

        public JobScheduler(ILogger<JobScheduler> logger, IJobFactory jobFactory, IMutex mutex)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            this.mutex = mutex ?? new InProcessMutex();
            this.jobFactory = jobFactory; // what to do when null?
        }

        public bool IsRunning => this.activeCount > 0;

        public IJobScheduler Register(string cron, Action<string[]> action) // TODO: not really needed
        {
            return this.Register(new JobRegistration(null, cron), new Job(action));
        }

        public IJobScheduler Register(string key, string cron, Action<string[]> action)
        {
            return this.Register(new JobRegistration(key, cron), new Job(action));
        }

        public IJobScheduler Register(string cron, Func<string[], Task> task)
        {
            return this.Register(new JobRegistration(null, cron), new Job(task));
        }

        public IJobScheduler Register(string key, string cron, Func<string[], Task> task)
        {
            return this.Register(new JobRegistration(key, cron), new Job(task));
        }

        public IJobScheduler Register<T>(string cron, string[] args = null)
            where T : IJob
        {
            return this.Register<T>(null, cron, args);
        }

        public IJobScheduler Register<T>(string key, string cron, string[] args = null)
            where T : IJob
        {
            if (!typeof(Job).IsAssignableFrom(typeof(T)))
            {
                throw new NaosException("Job type to register must implement IJob.");
            }

            return this.Register(
                new JobRegistration(key, cron, args),
                new Job(async (t, a) => // defer job creation
                {
                    var job = this.jobFactory.Create(typeof(T));
                    if(job == null)
                    {
                        throw new NaosException($"Cannot create job instance for type {typeof(T).PrettyName()}.");
                    }

                    await job.ExecuteAsync(t, a).ConfigureAwait(false);
                }));
        }

        public IJobScheduler Register<T>(string key, string cron, Expression<Func<T, Task>> task)
        {
            return this.Register(
                new JobRegistration(key, cron),
                new Job(() => // defer job creation
                {
                    var job = this.jobFactory.Create(typeof(T));
                    if (job == null)
                    {
                        throw new NaosException($"Cannot create job instance for type {typeof(T).PrettyName()}.");
                    }

                    var callExpression = task.Body as MethodCallExpression;
                    var method = callExpression.Method;
                    var args = callExpression.Arguments.Select(a => (a as ConstantExpression)?.Value);

                    method.Invoke(job, args.ToArray()); // this.GetExpressionValues(callExpression.Arguments)
                }));
        }

        public IJobScheduler Register(JobRegistration registration, IJob job)
        {
            EnsureArg.IsNotNull(registration, nameof(registration));
            EnsureArg.IsNotNullOrEmpty(registration.Cron, nameof(registration.Cron));

            if (job != null)
            {
                registration.Key = registration.Key ?? HashAlgorithm.ComputeHash(job);
                this.logger.LogInformation($"register scheduled job (key={registration.Key}, cron={registration.Cron})");
                this.registrations.Add(registration, job); // TODO: remove existing by key
            }

            return this;
        }

        public IJobScheduler UnRegister(string key)
        {
            return this.UnRegister(this.GetRegistationByKey(key));
        }

        public IJobScheduler UnRegister(JobRegistration registration)
        {
            if (registration != null)
            {
                this.registrations.Remove(registration);
            }

            return this;
        }

        public IJobScheduler OnError(Action<Exception> errorHandler)
        {
            this.errorHandler = errorHandler;
            return this;
        }

        public async Task TriggerAsync(string key, string[] args = null)
        {
            var item = this.registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(key));
            if (item.Key != null)
            {
                await this.TriggerAsync(key, new CancellationTokenSource(item.Key.Timeout).Token, args);
            }
            else
            {
                this.logger.LogInformation($"job scheduler does not have a job registered with key {key}");
            }
        }

        public async Task TriggerAsync(string key, CancellationToken token, string[] args = null)
        {
            var item = this.registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(key));
            if (item.Key != null)
            {
                await this.ExecuteJobAsync(item.Key, item.Value, token, args ?? item.Key?.Args).ConfigureAwait(false);
            }
            else
            {
                this.logger.LogInformation($"job scheduler does not have a job registered with key {key}");
            }
        }

        public async Task RunAsync() // TODO: a different token per job is better to cancel individual jobs (+ timeout)
        {
            await this.RunAsync(DateTime.UtcNow);
        }

        public async Task RunAsync(DateTime moment) // TODO: a different token per job is better to cancel individual jobs (+ timeout)
        {
            EnsureArg.IsTrue(moment.Kind == DateTimeKind.Utc);

            Interlocked.Increment(ref this.activeCount);
            this.logger.LogInformation($"job scheduler run started (activeCount=#{this.activeCount}, moment={moment.ToString("o")})");
            await this.ExecuteJobsAsync(moment).ConfigureAwait(false);
            Interlocked.Decrement(ref this.activeCount);
            this.logger.LogInformation($"job scheduler run finished (activeCount=#{this.activeCount})");
        }

private async Task ExecuteJobsAsync(DateTime moment)
        {
            var dueJobs = this.registrations.Where(t => t.Key?.IsDue(moment) == true).Select(r =>
            {
                var cts = new CancellationTokenSource(r.Key.Timeout);
                return Task.Run(async () =>
                {
                    await this.ExecuteJobAsync(r.Key, r.Value, cts.Token, r.Key.Args).ConfigureAwait(false);
                }, cts.Token);
            }).ToList();

            if (dueJobs.IsNullOrEmpty())
            {
                this.logger.LogInformation($"job scheduler run no due jobs at moment {moment.ToString("o")}");
            }

            await Task.WhenAll(dueJobs).ConfigureAwait(false); // really wait for completion (await)?
        }

        private async Task ExecuteJobAsync(JobRegistration registration, IJob job, CancellationToken token, string[] args = null)
        {
            if (registration?.Key.IsNullOrEmpty() == false && job != null)
            {
                try
                {
                    async Task Execute()
                    {
                        // TODO: publish domain event (job started)
                        this.logger.LogInformation($"job started (key={registration.Key}, type={job.GetType().PrettyName()})");
                        await job.ExecuteAsync(token, args).ConfigureAwait(false);
                        this.logger.LogInformation($"job finished (key={registration.Key}, type={job.GetType().PrettyName()})");
                        // TODO: publish domain event (job finished)
                    }

                    if (registration.PreventOverlap)
                    {
                        if (this.mutex.TryAcquireLock(registration.Key))
                        {
                            try
                            {
                                await Execute();
                            }
                            finally
                            {
                                this.mutex.ReleaseLock(registration.Key);
                            }
                        }
                        else
                        {
                            this.logger.LogWarning($"job already executing (key={registration.Key}, type={job.GetType().PrettyName()})");
                        }
                    }
                    else
                    {
                        await Execute();
                    }
                }
                catch (OperationCanceledException ex)
                {
                    // TODO: publish domain event (job failed)
                    this.logger.LogWarning(ex, $"job canceled (key={registration.Key}), type={job.GetType().PrettyName()}) [{ex.GetType().Name}] {ex.Message}");
                    //this.errorHandler?.Invoke(ex);
                }
                catch (Exception ex)
                {
                    // TODO: publish domain event (job failed)
                    this.logger.LogError(ex, $"job failed (key={registration.Key}), type={job.GetType().PrettyName()}) [{ex.GetType().Name}] {ex.Message}");
                    this.errorHandler?.Invoke(ex);
                }
            }
        }

        private JobRegistration GetRegistationByKey(string key)
        {
            return this.registrations.Where(r => r.Key.Key.SafeEquals(key)).Select(r => r.Key).FirstOrDefault();
        }
    }
}