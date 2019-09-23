namespace Naos.JobScheduling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.JobScheduling.Domain;

    public class JobSchedulerOptions
    {
        private readonly ILogger<JobSchedulerOptions> logger;
        private readonly IMediator mediator;
        private readonly IJobFactory jobFactory;

        public JobSchedulerOptions(ILoggerFactory loggerFactory, IMediator mediator, IJobFactory jobFactory)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));

            this.logger = loggerFactory.CreateLogger<JobSchedulerOptions>();
            this.mediator = mediator;
            this.jobFactory = jobFactory; // what to do when null?
        }

        public bool Enabled { get; private set; } = true;

        public IDictionary<JobRegistration, IJob> Registrations { get; } = new Dictionary<JobRegistration, IJob>();

        public JobSchedulerOptions SetEnabled(bool enabled = true)
        {
            this.Enabled = enabled;
            return this;
        }

        public JobSchedulerOptions Register(string cron, Action<string[]> action, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true) // TODO: not really needed
        {
            return this.Register(new JobRegistration(null, cron, null, isReentrant, timeout, enabled), new Job(action));
        }

        public JobSchedulerOptions Register(string key, string cron, Action<string[]> action, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
        {
            return this.Register(new JobRegistration(key, cron, null, isReentrant, timeout, enabled), new Job(action));
        }

        public JobSchedulerOptions Register(string cron, Func<string[], Task> task, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
        {
            return this.Register(new JobRegistration(null, cron, null, isReentrant, timeout, enabled), new Job(task));
        }

        public JobSchedulerOptions Register(string key, string cron, Func<string[], Task> task, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
        {
            return this.Register(new JobRegistration(key, cron, null, isReentrant, timeout, enabled), new Job(task));
        }

        public JobSchedulerOptions Register<T>(string cron, string[] args = null, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
            where T : IJob
        {
            return this.Register<T>(null, cron, args, isReentrant, timeout, enabled);
        }

        public JobSchedulerOptions Register<TRequest>(string cron, Func<TRequest> data, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
            where TRequest : class
        {
            return this.Register(null, cron, data, isReentrant, timeout, enabled);
        }

        public JobSchedulerOptions Register<TEvent>(string key, string cron, Func<TEvent> @event, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
            where TEvent : class
        {
            return this.Register(
               new JobRegistration(key, cron, null, isReentrant, timeout, enabled),
               new Job(async (t, a) => // send mediator request for data
                {
                    this.logger.LogInformation("{LogKey:l} send jobevent", LogKeys.JobScheduling);
                    if (@event != null)
                    {
                        await this.mediator.Send(new JobEvent<TEvent>(@event())).AnyContext();
                    }
                    else
                    {
                        await this.mediator.Send(new JobEvent<TEvent>(null)).AnyContext();
                    }
                }));
        }

        public JobSchedulerOptions Register<T>(string key, string cron, string[] args = null, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
            where T : IJob
        {
            if (!typeof(Job).IsAssignableFrom(typeof(T)))
            {
                throw new NaosException("Job type to register must implement IJob.");
            }

            return this.Register(
                new JobRegistration(key, cron, args, isReentrant, timeout, enabled),
                new Job(async (t, a) => // defer job creation
                {
                    var job = this.jobFactory.CreateJob(typeof(T));
                    if (job == null)
                    {
                        throw new NaosException($"Cannot create job instance for type {typeof(T).PrettyName()}.");
                    }

                    await job.ExecuteAsync(t, a).AnyContext();
                }));
        }

        public JobSchedulerOptions Register<T>(string key, string cron, Expression<Func<T, Task>> task, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
            where T : class
        {
            EnsureArg.IsNotNull(task, nameof(task));

            return this.Register(
                new JobRegistration(key, cron, null, isReentrant, timeout, enabled),
                new Job(async (t, a) => // defer job creation
                {
                    await Task.Run(() =>
                    {
                        var job = this.jobFactory.Create<T>();
                        if (job == null)
                        {
                            throw new NaosException($"cannot create job instance for type {typeof(T).PrettyName()}");
                        }

                        var callExpression = task.Body as MethodCallExpression;
                        callExpression?.Method.Invoke(
                            job,
                            callExpression?.Arguments?.Select(p => this.MapParameter(p, t)).ToArray());
                    }).AnyContext();
                }));
        }

        public JobSchedulerOptions Register(JobRegistration registration, IJob job)
        {
            EnsureArg.IsNotNull(registration, nameof(registration));
            EnsureArg.IsNotNull(job, nameof(job));

            registration.Key ??= HashAlgorithm.ComputeHash(job);
            this.logger.LogInformation($"{{LogKey:l}} registration (key={{JobKey}}, cron={registration.Cron}, isReentrant={registration.IsReentrant}, timeout={registration.Timeout.ToString("c")}, enabled={registration.Enabled})", LogKeys.JobScheduling, registration.Key);

            var item = this.Registrations.FirstOrDefault(r => r.Key.Key.SafeEquals(registration.Key));
            if (item.Key != null)
            {
                this.Registrations.Remove(item.Key);
            }

            this.Registrations.Add(registration, job);
            return this;
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