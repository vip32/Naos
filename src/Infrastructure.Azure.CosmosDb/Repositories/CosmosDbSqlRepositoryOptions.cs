namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using MediatR;
    using Naos.Core.Common;
    using Naos.Core.Domain;

    public class CosmosDbSqlRepositoryOptions<TEntity> : BaseOptions
        where TEntity : class, IEntity, IAggregateRoot
    {
        public IMediator Mediator { get; set; }

        public ICosmosDbSqlProvider<TEntity> Provider { get; set; }

        public bool PublishEvents { get; set; } = true;
    }
}