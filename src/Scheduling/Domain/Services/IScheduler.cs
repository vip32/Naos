namespace Naos.Core.Scheduling.Domain
{
    using System;
    using System.Threading.Tasks;

    public interface IScheduler
    {
        IScheduler OnError(Action<Exception> errorHandler);

        IScheduler Register(IScheduledTask scheduledTask);

        IScheduler Register(string cron, Action action);

        IScheduler Register(string cron, Func<Task> task);

        IScheduler Register(string key, IScheduledTask scheduledTask);

        IScheduler Register(string key, string cron, Action action);

        IScheduler Register(string key, string cron, Func<Task> task);

        IScheduler Register<T>(string cron)
            where T : IScheduledTask;

        IScheduler Register<T>(string key, string cron)
            where T : IScheduledTask;

        Task RunAsync();

        Task RunAsync(DateTime fromUtc);

        Task TriggerAsync(string key);

        IScheduler UnRegister(string key);
    }
}