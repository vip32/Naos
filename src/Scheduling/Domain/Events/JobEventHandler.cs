namespace Naos.Core.JobScheduling.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    public abstract class JobEventHandler<TData> : IRequestHandler<JobEvent<TData>, bool>
        where TData : class
    {
        public abstract Task<bool> Handle(JobEvent<TData> request, CancellationToken cancellationToken);
    }
}
