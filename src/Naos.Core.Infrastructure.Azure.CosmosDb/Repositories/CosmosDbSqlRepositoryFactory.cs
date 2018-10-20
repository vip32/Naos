namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    using MediatR;
    using Naos.Core.Domain;

    public static class CosmosDbSqlRepositoryFactory
    {
        //public static CosmosDbSqlRepository<TEntity> CreateForStringId<TEntity>(IMediator mediator, ICosmosDbSqlProvider<TEntity> provider)
        //    where TEntity : Entity<string>, IAggregateRoot
        //    => new CosmosDbSqlRepository<TEntity>(mediator, provider);
    }
}