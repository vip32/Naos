namespace Naos.Core.Scheduling.Domain
{
    using System;
    using System.Threading.Tasks;

    public interface IScheduledTask
    {
        string Cron { get; }

        bool IsDue(DateTime fromUtc, TimeSpan? span = null);

        bool IsDue(DateTime fromUtc, DateTime toUtc);

        Task ExecuteAsync(string[] args = null);
    }
}
