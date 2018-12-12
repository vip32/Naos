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
        private readonly IDictionary<JobRegistration, IJob> registrations = new Dictionary<JobRegistration, IJob>();
        private readonly ILogger<JobScheduler> logger;
        private readonly IMutex mutex;
        private int activeCount;
        private Action<Exception> errorHandler;

        public JobScheduler(ILogger<JobScheduler> logger, IMutex mutex)
            : this(logger, mutex, null)
        {
        }

        public JobScheduler(ILogger<JobScheduler> logger, IMutex mutex, JobSchedulerSettings settings)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(settings, nameof(settings));

            this.logger = logger;
            this.mutex = mutex ?? new InProcessMutex(null);
            this.registrations = settings.Registrations ?? new Dictionary<JobRegistration, IJob>();
        }

        public bool IsRunning => this.activeCount > 0;

        public IJobScheduler Register(JobRegistration registration, IJob job)
        {
            EnsureArg.IsNotNull(registration, nameof(registration));
            EnsureArg.IsNotNullOrEmpty(registration.Cron, nameof(registration.Cron));
            EnsureArg.IsNotNull(job, nameof(job));

            registration.Key = registration.Key ?? HashAlgorithm.ComputeHash(job);
            this.logger.LogInformation($"register scheduled job (key={registration.Key}, cron={registration.Cron}, isReentrant={registration.IsReentrant}, timeout={registration.Timeout.ToString("c")}, enabled={registration.Enabled})");

            var item = this.registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(registration.Key));
            if (item.Key != null)
            {
                this.registrations.Remove(item.Key);
            }

            this.registrations.Add(registration, job);
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

        public async Task TriggerAsync(string key, CancellationToken cancellationToken, string[] args = null)
        {
            var item = this.registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(key));
            if (item.Key != null)
            {
                await this.ExecuteJobAsync(item.Key, item.Value, cancellationToken, args ?? item.Key?.Args).ConfigureAwait(false);
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
            var dueJobs = this.registrations.Where(r => r.Key?.IsDue(moment) == true && r.Key.Enabled).Select(r =>
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

        private async Task ExecuteJobAsync(JobRegistration registration, IJob job, CancellationToken cancellationToken, string[] args = null)
        {
            if (registration?.Key.IsNullOrEmpty() == false && job != null)
            {
                try
                {
                    async Task Execute()
                    {
                        // TODO: publish domain event (job started)
                        this.logger.LogInformation($"job started (key={registration.Key}, type={job.GetType().PrettyName()}, isReentrant={registration.IsReentrant}, timeout={registration.Timeout.ToString("c")})");
                        await job.ExecuteAsync(cancellationToken, args).ConfigureAwait(false);
                        this.logger.LogInformation($"job finished (key={registration.Key}, type={job.GetType().PrettyName()})");
                        // TODO: publish domain event (job finished)
                    }

                    if (!registration.IsReentrant)
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
                    this.logger.LogWarning(ex, $"job canceled (key={registration.Key}), type={job.GetType().PrettyName()})");
                    //this.errorHandler?.Invoke(ex);
                }
                catch (Exception ex)
                {
                    // TODO: publish domain event (job failed)
                    this.logger.LogError(ex.InnerException ?? ex, $"job failed (key={registration.Key}), type={job.GetType().PrettyName()})");
                    this.errorHandler?.Invoke(ex.InnerException ?? ex);
                }
            }
        }

        private JobRegistration GetRegistationByKey(string key)
        {
            return this.registrations.Where(r => r.Key.Key.SafeEquals(key)).Select(r => r.Key).FirstOrDefault();
        }

        private object MapParameter(Expression expression, CancellationToken cancellationToken)
        {
            // https://gist.github.com/i-e-b/8556753
            if (expression.Type.IsValueType && expression.Type == typeof(CancellationToken))
            {
                return cancellationToken;
            }

            var objectMember = Expression.Convert(expression, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }
    }
}