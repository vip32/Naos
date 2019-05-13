namespace Naos.Core.Domain.Repositories
{
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class InMemoryRepositoryOptions<TEntity> : BaseOptions
        where TEntity : class, IEntity, IAggregateRoot
    {
        public IMediator Mediator { get; set; }

        public ICosmosDbSqlProvider<TEntity> Provider { get; set; }

        public bool PublishEvents { get; set; } = true;
    }
}