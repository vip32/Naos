namespace Naos.Foundation.Infrastructure
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Domain;

    public class SqlServerDocumentRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly SqlServerDocumentRepositoryOptions<TEntity> options;

        public SqlServerDocumentRepository(SqlServerDocumentRepositoryOptions<TEntity> options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Provider, nameof(options.Provider));

            this.options = options;
            this.Logger = options.CreateLogger<SqlServerDocumentRepository<TEntity>>();

            this.Logger.LogInformation($"{{LogKey:l}} construct sql document repository (type={typeof(TEntity).PrettyName()})", LogKeys.DomainRepository);
        }

        public SqlServerDocumentRepository(Builder<SqlServerDocumentRepositoryOptionsBuilder<TEntity>, SqlServerDocumentRepositoryOptions<TEntity>> optionsBuilder)
            : this(optionsBuilder(new SqlServerDocumentRepositoryOptionsBuilder<TEntity>()).Build())
        {
        }

        public ILogger<SqlServerDocumentRepository<TEntity>> Logger { get; }

        public Task<ActionResult> DeleteAsync(object id)
        {
            throw new System.NotImplementedException();
        }

        public Task<ActionResult> DeleteAsync(TEntity entity)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> ExistsAsync(object id)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task<TEntity> FindOneAsync(object id)
        {
            throw new System.NotImplementedException();
        }

        public Task<TEntity> InsertAsync(TEntity entity)
        {
            throw new System.NotImplementedException();
        }

        public Task<TEntity> UpdateAsync(TEntity entity)
        {
            throw new System.NotImplementedException();
        }

        public Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            throw new System.NotImplementedException();
        }
    }
}
