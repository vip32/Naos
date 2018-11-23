namespace Naos.Core.Scheduling.Domain
{
    using System;
    using System.Threading.Tasks;

    public interface IScheduler
    {
        bool IsRunning { get; }

        IScheduler OnError(Action<Exception> errorHandler);

        IScheduler Register(Registration registration, IScheduledTask task);

        IScheduler Register(string cron, Action<string[]> action);

        IScheduler Register(string key, string cron, Action<string[]> action);

        IScheduler Register(string cron, Func<string[], Task> func);

        IScheduler Register(string key, string cron, Func<string[], Task> func);

        IScheduler Register<T>(string cron, string[] args = null)
            where T : IScheduledTask;

        IScheduler Register<T>(string key, string cron, string[] args = null)
            where T : IScheduledTask;

        Task RunAsync();

        Task RunAsync(DateTime moment);

        Task TriggerAsync(string key);

        IScheduler UnRegister(string key);

        IScheduler UnRegister(Registration registration);
    }
}