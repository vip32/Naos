namespace Naos.Core.JobScheduling.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IJobScheduler
    {
        bool IsRunning { get; }

        JobSchedulerOptions Options { get; }

        IJobScheduler OnError(Action<Exception> errorHandler);

        IJobScheduler Register(JobRegistration registration, IJob job);

        Task RunAsync();

        Task RunAsync(DateTime moment);

        Task TriggerAsync(string key, string[] args = null);

        Task TriggerAsync(string key, CancellationToken cancellationToken, string[] args = null);

        IJobScheduler UnRegister(string key);

        IJobScheduler UnRegister(JobRegistration registration);
    }
}