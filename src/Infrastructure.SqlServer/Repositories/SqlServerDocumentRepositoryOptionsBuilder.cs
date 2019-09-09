namespace Naos.Foundation.Infrastructure
{
    using Naos.Foundation.Domain;

    public class SqlServerDocumentRepositoryOptionsBuilder<TEntity> :
        BaseOptionsBuilder<SqlServerDocumentRepositoryOptions<TEntity>, SqlServerDocumentRepositoryOptionsBuilder<TEntity>>
        where TEntity : class, IEntity, IAggregateRoot
    {
        public SqlServerDocumentRepositoryOptionsBuilder<TEntity> Provider(SqlServerDocumentProvider<TEntity> provider)
        {
            this.Target.Provider = provider;
            return this;
        }
    }
}