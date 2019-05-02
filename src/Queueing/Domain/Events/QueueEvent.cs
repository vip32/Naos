namespace Naos.Core.Queueing.Domain
{
    using System;
    using MediatR;
    using Naos.Core.Domain;

    public class QueueEvent<TData> : IRequest<bool>
        where TData : class
    {
        public QueueEvent(IQueueItem<TData> item)
        {
            this.Created = DateTime.UtcNow;
            this.Item = item;
        }

        public DateTime Created { get; }

        public DataDictionary Properties { get; set; } = new DataDictionary();

        public IQueueItem<TData> Item { get; }
    }
}
