namespace Naos.Core.Queueing.Domain
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;
    using Naos.Core.Domain.Model;

    public class QueueItemRequest<TData> : IRequest<bool>
        where TData : class
    {
        public QueueItemRequest(IQueueItem<TData> item)
        {
            this.Created = DateTime.UtcNow;
            this.Item = item;
        }

        public DateTime Created { get; }

        public DataDictionary Properties { get; set; } = new DataDictionary();

        public IQueueItem<TData> Item { get; }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public abstract class BaseQueueItemRequestHandler<TRequest, TData> : IRequestHandler<TRequest, bool>
#pragma warning restore SA1402 // File may only contain a single class
        where TRequest : QueueItemRequest<TData>
        where TData : class
    {
        public abstract Task<bool> Handle(TRequest request, CancellationToken cancellationToken);
    }
}
