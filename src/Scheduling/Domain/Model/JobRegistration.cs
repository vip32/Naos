namespace Naos.Core.Scheduling.Domain
{
    using System;
    using System.Linq;
    using Cronos;
    using EnsureThat;

    public class JobRegistration
    {
        private readonly CronExpression cronExpression;

        public JobRegistration(string key, string cron, string[] args = null, bool isReentrant = false, TimeSpan? timeout = null)
        {
            EnsureArg.IsNotNullOrEmpty(cron, nameof(cron));

            this.Key = key;
            this.Cron = cron;
            this.Args = args;
            this.IsReentrant = isReentrant;
            this.Timeout = timeout ?? new TimeSpan(0, 20, 0);
            if (cron.Count(char.IsWhiteSpace) == 4) // mi ho da mo yy
            {
                this.cronExpression = CronExpression.Parse(this.Cron, CronFormat.Standard);
            }
            else
            {
                this.cronExpression = CronExpression.Parse(this.Cron, CronFormat.IncludeSeconds);
            }
        }

        public string Key { get; set; }

        public string Cron { get; }

        public string[] Args { get; }

        public bool IsReentrant { get; }

        public TimeSpan Timeout { get; }

        public bool IsDue(DateTime fromUtc, TimeSpan? span = null)
        {
            EnsureArg.IsTrue(fromUtc.Kind == DateTimeKind.Utc);

            span = span ?? TimeSpan.FromMinutes(1);
            var occurrence = this.cronExpression.GetNextOccurrence(fromUtc, true);

            if (!occurrence.HasValue)
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

            return this.cronExpression.GetOccurrences(fromUtc, toUtc, true)?.Any() == true;
        }
    }
}
