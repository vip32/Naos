namespace Naos.Core.Scheduling.Domain
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cronos;
    using EnsureThat;

    public class ScheduledTask : IScheduledTask
    {
        private readonly string cron;
        private readonly Type type;
        private readonly Func<Task> task;
        private readonly Action action;

        public ScheduledTask(string cron)
        {
            EnsureArg.IsNotNullOrEmpty(cron, nameof(cron));

            this.cron = cron;
        }

        public ScheduledTask(string cron, Action action)
        {
            EnsureArg.IsNotNullOrEmpty(cron, nameof(cron));
            EnsureArg.IsNotNull(action, nameof(action));

            this.cron = cron;
            this.action = action;
        }

        public ScheduledTask(string cron, Func<Task> task)
        {
            EnsureArg.IsNotNullOrEmpty(cron, nameof(cron));
            EnsureArg.IsNotNull(task, nameof(task));

            this.cron = cron;
            this.task = task;
        }

        public ScheduledTask(string cron, Type type)
        {
            EnsureArg.IsNotNullOrEmpty(cron, nameof(cron));
            EnsureArg.IsTrue(type.IsAssignableFrom(typeof(IScheduledTask)), nameof(type));

            this.cron = cron;
            this.type = type;
        }

        // TODO: Other things to schedule
        // Schedule - Command
        // Schedule - Message

        public bool IsDue(DateTime fromUtc, TimeSpan? span = null)
        {
            EnsureArg.IsTrue(fromUtc.Kind == DateTimeKind.Utc);

            span = span ?? TimeSpan.FromMinutes(1);

            var expression = CronExpression.Parse(this.cron, CronFormat.IncludeSeconds);
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

            var expression = CronExpression.Parse(this.cron, CronFormat.IncludeSeconds);
            var occurrences = expression.GetOccurrences(fromUtc, toUtc, true);

            return occurrences?.Any() == true;
        }

        public virtual async Task ExecuteAsync()
        {
            if(this.task != null)
            {
                await this.task();
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
