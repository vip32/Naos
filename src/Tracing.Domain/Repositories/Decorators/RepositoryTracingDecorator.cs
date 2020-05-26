namespace Naos.Tracing.Domain
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;

    /// <summary>
    /// <para>Decorates an <see cref="Repositories.IGenericRepository{TEntity}"/>.</para>
    /// <para>
    ///    .-----------.
    ///    | Decorator |
    ///    .-----------.        .------------.
    ///          `------------> | decoratee  |
    ///            (forward)    .------------.
    /// </para>
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <seealso cref="Repositories.IGenericRepository{TEntity}" />
    public class RepositoryTracingDecorator<TEntity> : IGenericRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        private readonly ITracer tracer;
        private readonly ILogger<IGenericRepository<TEntity>> logger;
        private readonly IGenericRepository<TEntity> decoratee;
        private readonly string name;

        public RepositoryTracingDecorator(
            ILogger<IGenericRepository<TEntity>> logger,
            ITracer tracer,
            IGenericRepository<TEntity> decoratee)
        {
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.tracer = tracer;
            this.logger = logger;
            this.decoratee = decoratee;
            this.name = typeof(TEntity).Name.ToLower();
        }

        public async Task<RepositoryActionResult> DeleteAsync(object id)
        {
            using (var scope = this.tracer?.BuildSpan($"delete {this.name}", LogKeys.DomainRepository)
                .WithTag(SpanTagKey.DbType, "sql").Activate(this.logger))
            {
                return await this.decoratee.DeleteAsync(id).AnyContext();
            }
        }

        public async Task<RepositoryActionResult> DeleteAsync(TEntity entity)
        {
            using (var scope = this.tracer?.BuildSpan($"delete {this.name}", LogKeys.DomainRepository)
                .WithTag(SpanTagKey.DbType, "sql").Activate(this.logger))
            {
                return await this.decoratee.DeleteAsync(entity).AnyContext();
            }
        }

        public async Task<bool> ExistsAsync(object id)
        {
            using (var scope = this.tracer?.BuildSpan($"exists {this.name}", LogKeys.DomainRepository)
                .WithTag(SpanTagKey.DbType, "sql").Activate(this.logger))
            {
                return await this.decoratee.ExistsAsync(id).AnyContext();
            }
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            using (var scope = this.tracer?.BuildSpan($"findall {this.name}", LogKeys.DomainRepository)
                .WithTag(SpanTagKey.DbType, "sql").Activate(this.logger))
            {
                return await this.decoratee.FindAllAsync(options, cancellationToken).AnyContext();
            }
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            using (var scope = this.tracer?.BuildSpan($"findall {this.name}", LogKeys.DomainRepository)
                .WithTag(SpanTagKey.DbType, "sql").Activate(this.logger))
            {
                return await this.decoratee.FindAllAsync(specification, options, cancellationToken).AnyContext();
            }
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            using (var scope = this.tracer?.BuildSpan($"findall {this.name}", LogKeys.DomainRepository)
                .WithTag(SpanTagKey.DbType, "sql").Activate(this.logger))
            {
                return await this.decoratee.FindAllAsync(specifications, options, cancellationToken).AnyContext();
            }
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            using (var scope = this.tracer?.BuildSpan($"findone {this.name}", LogKeys.DomainRepository)
                .WithTag(SpanTagKey.DbType, "sql").Activate(this.logger))
            {
                return await this.decoratee.FindOneAsync(id).AnyContext();
            }
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            using (var scope = this.tracer?.BuildSpan($"insert {this.name}", LogKeys.DomainRepository)
                .WithTag(SpanTagKey.DbType, "sql").Activate(this.logger))
            {
                return await this.decoratee.InsertAsync(entity).AnyContext();
            }
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            using (var scope = this.tracer?.BuildSpan($"update {this.name}", LogKeys.DomainRepository)
                .WithTag(SpanTagKey.DbType, "sql").Activate(this.logger))
            {
                return await this.decoratee.UpdateAsync(entity).AnyContext();
            }
        }

        public async Task<(TEntity entity, RepositoryActionResult action)> UpsertAsync(TEntity entity)
        {
            using (var scope = this.tracer?.BuildSpan($"upsert {this.name}", LogKeys.DomainRepository)
                .WithTag(SpanTagKey.DbType, "sql").Activate(this.logger))
            {
                return await this.decoratee.UpsertAsync(entity).AnyContext();
            }
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            using (var scope = this.tracer?.BuildSpan($"count {this.name}", LogKeys.DomainRepository)
                .WithTag(SpanTagKey.DbType, "sql").Activate(this.logger))
            {
                return await this.decoratee.CountAsync(cancellationToken).AnyContext();
            }
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="specification">The specification.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(ISpecification<TEntity> specification, CancellationToken cancellationToken = default)
        {
            using (var scope = this.tracer?.BuildSpan($"count {this.name}", LogKeys.DomainRepository)
                .WithTag(SpanTagKey.DbType, "sql").Activate(this.logger))
            {
                return await this.decoratee.CountAsync(specification, cancellationToken).AnyContext();
            }
        }

        /// <summary>
        /// Counts all entities.
        /// </summary>
        /// <param name="specifications">The specifications.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public async Task<int> CountAsync(IEnumerable<ISpecification<TEntity>> specifications, CancellationToken cancellationToken = default)
        {
            using (var scope = this.tracer?.BuildSpan($"count {this.name}", LogKeys.DomainRepository)
                .WithTag(SpanTagKey.DbType, "sql").Activate(this.logger))
            {
                return await this.decoratee.CountAsync(specifications, cancellationToken).AnyContext();
            }
        }
    }
}
