namespace Naos.Queueing.Domain
{
    using System.Threading;
    using System.Threading.Tasks;
    using MediatR;

    public abstract class QueueEventHandler<TData> : IRequestHandler<QueueEvent<TData>, bool>
        where TData : class
    {
        public abstract Task<bool> Handle(QueueEvent<TData> request, CancellationToken cancellationToken);
    }
}
