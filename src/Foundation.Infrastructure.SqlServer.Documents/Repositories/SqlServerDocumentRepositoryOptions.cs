namespace Naos.Foundation.Infrastructure
{
    using Naos.Foundation.Domain;

    public class SqlServerDocumentRepositoryOptions<TEntity> : BaseOptions
        where TEntity : class, IEntity, IAggregateRoot
    {
        public SqlServerDocumentProvider<TEntity> Provider { get; set; }
    }
}
