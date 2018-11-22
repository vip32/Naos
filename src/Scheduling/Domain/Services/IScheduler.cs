namespace Naos.Core.Scheduling.Domain.Services
{
    using System;
    using System.Threading.Tasks;

    public interface IScheduler
    {
        IScheduler Register(string key, string cron, Action action);

        IScheduler Register(string key, string cron, Func<Task> task);

        IScheduler Register<T>(string key, string cron)
            where T : IScheduledTask;
    }
}