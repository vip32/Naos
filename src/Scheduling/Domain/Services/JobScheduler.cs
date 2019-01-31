namespace Naos.Core.JobScheduling.Domain
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;

    public class JobScheduler : IJobScheduler
    {
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
            this.Settings = settings;
        }

        public bool IsRunning => this.activeCount > 0;

        public JobSchedulerSettings Settings { get; }

        public IJobScheduler Register(JobRegistration registration, IJob job)
        {
            EnsureArg.IsNotNull(registration, nameof(registration));
            EnsureArg.IsNotNullOrEmpty(registration.Cron, nameof(registration.Cron));
            EnsureArg.IsNotNull(job, nameof(job));

            registration.Key = registration.Key ?? HashAlgorithm.ComputeHash(job);
            this.logger.LogInformation($"{{LogKey:l}} registration (key={{JobKey}}, cron={registration.Cron}, isReentrant={registration.IsReentrant}, timeout={registration.Timeout.ToString("c")}, enabled={registration.Enabled})", LogEventKeys.JobScheduling, registration.Key);

            var item = this.Settings.Registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(registration.Key));
            if (item.Key != null)
            {
                this.Settings.Registrations.Remove(item.Key);
            }

            this.Settings.Registrations.Add(registration, job);
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
                this.Settings.Registrations.Remove(registration);
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
            var item = this.Settings.Registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(key));
            if (item.Key != null)
            {
                await this.TriggerAsync(key, new CancellationTokenSource(item.Key.Timeout).Token, args);
            }
            else
            {
                this.logger.LogInformation("{LogKey:l} does not known registration with key {JobKey} registered", LogEventKeys.JobScheduling, key);
            }
        }

        public async Task TriggerAsync(string key, CancellationToken cancellationToken, string[] args = null)
        {
            var item = this.Settings.Registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(key));
            if (item.Key != null)
            {
                await this.ExecuteJobAsync(item.Key, item.Value, cancellationToken, args ?? item.Key?.Args).ConfigureAwait(false);
            }
            else
            {
                this.logger.LogInformation("{LogKey:l} does not know registration with key {JobKey} registered", LogEventKeys.JobScheduling, key);
            }
        }

        public async Task RunAsync() // TODO: a different token per job is better to cancel individual jobs (+ timeout)
        {
            await this.RunAsync(DateTime.UtcNow);
        }

        public async Task RunAsync(DateTime moment) // TODO: a different token per job is better to cancel individual jobs (+ timeout)
        {
            EnsureArg.IsTrue(moment.Kind == DateTimeKind.Utc);

            if (!this.Settings.Enabled)
            {
                //this.logger.LogDebug($"job scheduler run not started (enabled={this.Settings.Enabled})");
                return;
            }

            Interlocked.Increment(ref this.activeCount);
            this.logger.LogInformation($"{{LogKey:l}} run started (activeCount=#{this.activeCount}, moment={moment.ToString("o")})", LogEventKeys.JobScheduling);
            await this.ExecuteJobsAsync(moment).ConfigureAwait(false);
            Interlocked.Decrement(ref this.activeCount);
            this.logger.LogInformation($"{{LogKey:l}} run finished (activeCount=#{this.activeCount})", LogEventKeys.JobScheduling);
        }

        private async Task ExecuteJobsAsync(DateTime moment)
        {
            var dueJobs = this.Settings.Registrations.Where(r => r.Key?.IsDue(moment) == true && r.Key.Enabled).Select(r =>
            {
                var cts = new CancellationTokenSource(r.Key.Timeout);
                return Task.Run(async () =>
                {
                    await this.ExecuteJobAsync(r.Key, r.Value, cts.Token, r.Key.Args).ConfigureAwait(false);
                }, cts.Token);
            }).ToList();

            if (dueJobs.IsNullOrEmpty())
            {
                this.logger.LogInformation($"{{LogKey:l}} run has no due jobs at moment {moment.ToString("o")}", LogEventKeys.JobScheduling);
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
                        this.logger.LogJournal(LogEventPropertyKeys.TrackStartJob, $"{{LogKey:l}} [{registration.Identifier}] job started (key={{JobKey}}, type={job.GetType().PrettyName()}, isReentrant={registration.IsReentrant}, timeout={registration.Timeout.ToString("c")})", args: new[] { LogEventKeys.JobScheduling, registration.Key });
                        await job.ExecuteAsync(cancellationToken, args).ConfigureAwait(false);
                        this.logger.LogJournal(LogEventPropertyKeys.TrackFinishJob, $"{{LogKey:l}} [{registration.Identifier}] job finished (key={{JobKey}}, type={job.GetType().PrettyName()})", args: new[] { LogEventKeys.JobScheduling, registration.Key });
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
                            this.logger.LogWarning($"{{LogKey:l}} already executing (key={{JobKey}}, type={job.GetType().PrettyName()})", LogEventKeys.JobScheduling, registration.Key);
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
                    this.logger.LogWarning(ex, $"{{LogKey:l}} canceled (key={{JobKey}}), type={job.GetType().PrettyName()})", LogEventKeys.JobScheduling, registration.Key);
                    //this.errorHandler?.Invoke(ex);
                }
                catch (Exception ex)
                {
                    // TODO: publish domain event (job failed)
                    this.logger.LogError(ex.InnerException ?? ex, $"{{LogKey:l}} failed (key={{JobKey}}), type={job.GetType().PrettyName()})", LogEventKeys.JobScheduling, registration.Key);
                    this.errorHandler?.Invoke(ex.InnerException ?? ex);
                }
            }
        }

        private JobRegistration GetRegistationByKey(string key)
        {
            return this.Settings.Registrations.Where(r => r.Key.Key.SafeEquals(key)).Select(r => r.Key).FirstOrDefault();
        }
    }
}