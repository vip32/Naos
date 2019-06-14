namespace Naos.Foundation.Infrastructure
{
    using MediatR;
    using Naos.Foundation.Domain;

    public class CosmosDbSqlRepositoryOptions<TEntity> : BaseOptions
        where TEntity : class, IEntity, IAggregateRoot
    {
        public IMediator Mediator { get; set; }

        public ICosmosDbSqlProvider<TEntity> Provider { get; set; }

        public bool PublishEvents { get; set; } = true;
    }
}