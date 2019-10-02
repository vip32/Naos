namespace Naos.JobScheduling.Domain
{
    using System;
    using System.Linq;
    using Cronos;
    using EnsureThat;
    using Naos.Foundation;

    public class JobRegistration
    {
        private readonly CronExpression cronExpression;

        public JobRegistration(string key, string cron, string[] args = null, bool isReentrant = false, TimeSpan? timeout = null, bool enabled = true)
        {
            this.Key = key;
            this.Cron = cron;
            this.Args = args;
            this.IsReentrant = isReentrant;
            this.Timeout = timeout ?? new TimeSpan(0, 20, 0);
            this.Enabled = enabled;
            this.Identifier = RandomGenerator.GenerateString(5, false);

            if (!this.Cron.IsNullOrEmpty())
            {
                if (this.Cron.Count(char.IsWhiteSpace) == 4) // mi ho da mo yy
                {
                    this.cronExpression = CronExpression.Parse(this.Cron, CronFormat.Standard);
                }
                else
                {
                    this.cronExpression = CronExpression.Parse(this.Cron, CronFormat.IncludeSeconds);
                }
            }
        }

        public string Key { get; set; }

        public string Identifier { get; }

        public string Cron { get; }

        public string[] Args { get; }

        public bool IsReentrant { get; }

        public bool Enabled { get; }

        public TimeSpan Timeout { get; }

        public bool IsDue(DateTime moment, TimeSpan? span = null)
        {
            EnsureArg.IsTrue(moment.Kind == DateTimeKind.Utc);

            if (this.cronExpression == null)
            {
                return false;
            }

            span ??= TimeSpan.FromMinutes(1);
            var occurrence = this.cronExpression.GetNextOccurrence(moment, true);

            if (!occurrence.HasValue)
            {
                return false;
            }

            return occurrence.Value - moment < span;
        }

        public bool IsDue(DateTime fromUtc, DateTime toUtc)
        {
            EnsureArg.IsTrue(fromUtc.Kind == DateTimeKind.Utc);
            EnsureArg.IsTrue(toUtc.Kind == DateTimeKind.Utc);
            EnsureArg.IsTrue(fromUtc < toUtc);

            if (this.cronExpression == null)
            {
                return false;
            }

            return this.cronExpression.GetOccurrences(fromUtc, toUtc, true)?.Any() == true;
        }
    }
}
