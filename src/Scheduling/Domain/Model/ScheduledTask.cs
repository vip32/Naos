namespace Naos.Core.Scheduling.Domain
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cronos;
    using EnsureThat;

    public class ScheduledTask : IScheduledTask
    {
        private readonly Func<Task> func;
        private readonly Action action;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledTask"/> class.
        /// </summary>
        /// <param name="cron">The cron.</param>
        public ScheduledTask(string cron)
        {
            EnsureArg.IsNotNullOrEmpty(cron, nameof(cron));

            this.Cron = cron;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledTask"/> class.
        /// </summary>
        /// <param name="cron">The cron expression.</param>
        /// <param name="action">The action.</param>
        public ScheduledTask(string cron, Action action)
        {
            EnsureArg.IsNotNullOrEmpty(cron, nameof(cron));
            EnsureArg.IsNotNull(action, nameof(action));

            this.Cron = cron;
            this.action = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScheduledTask"/> class.
        /// </summary>
        /// <param name="cron">The cron expression.</param>
        /// <param name="func">The func task.</param>
        public ScheduledTask(string cron, Func<Task> func)
        {
            EnsureArg.IsNotNullOrEmpty(cron, nameof(cron));
            EnsureArg.IsNotNull(func, nameof(func));

            this.Cron = cron;
            this.func = func;
        }

        // TODO: Other things to schedule
        // Schedule - Command
        // Schedule - Message
        public string Cron { get; private set; }

        public bool IsDue(DateTime fromUtc, TimeSpan? span = null)
        {
            EnsureArg.IsTrue(fromUtc.Kind == DateTimeKind.Utc);

            span = span ?? TimeSpan.FromMinutes(1);

            var expression = CronExpression.Parse(this.Cron, CronFormat.IncludeSeconds);
            var occurrence = expression.GetNextOccurrence(fromUtc, true);

            if(!occurrence.HasValue)
            {
                return false;
            }

            return occurrence.Value - fromUtc < span;
        }

        public bool IsDue(DateTime fromUtc, DateTime toUtc)
        {
            EnsureArg.IsTrue(fromUtc.Kind == DateTimeKind.Utc);
            EnsureArg.IsTrue(toUtc.Kind == DateTimeKind.Utc);
            EnsureArg.IsTrue(fromUtc < toUtc);

            var expression = CronExpression.Parse(this.Cron, CronFormat.IncludeSeconds);
            var occurrences = expression.GetOccurrences(fromUtc, toUtc, true);

            return occurrences?.Any() == true;
        }

        public virtual async Task ExecuteAsync()
        {
            if(this.func != null)
            {
                await this.func().ConfigureAwait(false);
            }
            else if (this.action != null)
            {
                this.action();
            }
        }

        // TODO: action/func/type containing the thing to invoke when due
    }

#pragma warning disable SA1402 // File may only contain a single class
    public abstract class ScheduledTaskImpl : ScheduledTask
#pragma warning restore SA1402 // File may only contain a single class
    {
        public ScheduledTaskImpl(string cron)
            : base(cron)
        {
        }

        public override abstract Task ExecuteAsync();
    }
}
