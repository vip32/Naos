namespace Naos.Core.Queueing.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    public abstract class BaseQueueItemRequestHandler<TData> : IRequestHandler<QueueItemRequest<TData>, bool>
        where TData : class
    {
        public abstract Task<bool> Handle(QueueItemRequest<TData> request, CancellationToken cancellationToken);
    }
}
