namespace Naos.Core.Queueing.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    public abstract class BaseQueueItemRequestHandler<TRequest, TData> : IRequestHandler<TRequest, bool>
        where TRequest : QueueItemRequest<TData>
        where TData : class
    {
        public abstract Task<bool> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
