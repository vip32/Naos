namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;
    using Naos.Foundation.Domain;

    public class MongoDbRepository<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly ILogger<IGenericRepository<TEntity>> logger;
        private readonly MongoDbRepositoryOptions options;

        public MongoDbRepository(MongoDbRepositoryOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Client, nameof(options.Client));
            EnsureArg.IsNotNullOrEmpty(options.Database, nameof(options.Database));

            this.options = options;
            this.logger = options.CreateLogger<IGenericRepository<TEntity>>();

            this.Collection = options.Client.GetDatabase(options.Database).GetCollection<TEntity>(typeof(TEntity).Name);
        }

        public MongoDbRepository(Builder<MongoDbRepositoryOptionsBuilder, MongoDbRepositoryOptions> optionsBuilder)
            : this(optionsBuilder(new MongoDbRepositoryOptionsBuilder()).Build())
        {
        }

        protected IMongoCollection<TEntity> Collection { get;  }

        public async Task<bool> ExistsAsync(object id)
        {
            return (await this.FindOneAsync(id).AnyContext()) != null;
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            return await this.Collection.Find(e => e.Id.Equals(id)).SingleOrDefaultAsync().AnyContext();
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            return await this.Collection.Find(e => e != null).ToListAsync().AnyContext();
            // TODO: skip/take https://www.codementor.io/pmbanugo/working-with-mongodb-in-net-part-3-skip-sort-limit-and-projections-oqfwncyka
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            if (specification == null)
            {
                return await this.FindAllAsync(options, cancellationToken).AnyContext();
            }

            return await this.Collection.Find(specification?.ToExpression()).ToListAsync().AnyContext();
            // TODO: skip/take https://www.codementor.io/pmbanugo/working-with-mongodb-in-net-part-3-skip-sort-limit-and-projections-oqfwncyka
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            if (specifications.IsNullOrEmpty())
            {
                return await this.FindAllAsync(options, cancellationToken).AnyContext();
            }

            if (specifications.Count() == 1)
            {
                return await this.FindAllAsync(specifications.First(), options, cancellationToken).AnyContext();
            }

            return await this.FindAllAsync(specifications.First().And(specifications.Skip(1)), options, cancellationToken).AnyContext();
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            await this.Collection.InsertOneAsync(entity).AnyContext();
            return entity;
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            await this.Collection.ReplaceOneAsync(e => e.Id.Equals(entity.Id), entity).AnyContext();
            return entity;
        }

        public Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            // TODO: insert or update
            throw new NotImplementedException();
        }

        public async Task<ActionResult> DeleteAsync(object id)
        {
            var result = await this.Collection.DeleteOneAsync(e => e.Id.Equals(id)).AnyContext();
            return result.DeletedCount > 0 ? ActionResult.Deleted : ActionResult.None;
        }

        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            return await this.DeleteAsync(entity.Id).AnyContext();
        }
    }
}
