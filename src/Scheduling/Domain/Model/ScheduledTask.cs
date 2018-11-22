namespace Naos.Core.Scheduling.Domain
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Cronos;
    using EnsureThat;

    public class ScheduledTask
    {
        private readonly string cron;
        private readonly Func<Task> task;
        private readonly Action action;

        public ScheduledTask(string cron, Func<Task> task = null, Action action = null)
        {
            this.cron = cron;
            this.task = task;
            this.action = action;
        }

        public bool IsDue(DateTime fromUtc, TimeSpan? span = null)
        {
            EnsureArg.IsTrue(fromUtc.Kind == DateTimeKind.Utc);

            span = span ?? TimeSpan.FromMinutes(1);

            var expression = CronExpression.Parse(this.cron, CronFormat.IncludeSeconds);
            var occurrence = expression.GetNextOccurrence(fromUtc);

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
            var occurrences = expression.GetOccurrences(fromUtc, toUtc);

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
            : base(cron, null, null)
        {
        }

        public override abstract Task ExecuteAsync();
    }
}
