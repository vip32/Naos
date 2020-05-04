namespace Naos.Foundation.Domain
{
    using System;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using MediatR;
    using Naos.Foundation.Domain.EventSourcing;

    public interface ISnapshotRepository<TAggregate, TId>
        where TAggregate : EventSourcedAggregateRoot<TId>, IEventSourcedAggregateRoot<TId>
    {
        public Task<TAggregate> GetByIdAsync(TId aggregateId);

        public Task SaveAsync(TAggregate aggregate);
    }
}
