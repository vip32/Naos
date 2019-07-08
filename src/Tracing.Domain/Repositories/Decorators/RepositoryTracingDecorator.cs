namespace Naos.Core.Tracing.Domain
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
        private readonly IGenericRepository<TEntity> decoratee;

        public RepositoryTracingDecorator(
            ITracer tracer,
            IGenericRepository<TEntity> decoratee)
        {
            EnsureArg.IsNotNull(tracer, nameof(tracer));
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.tracer = tracer;
            this.decoratee = decoratee;
        }

        public async Task<ActionResult> DeleteAsync(object id)
        {
            using(var scope = this.tracer?.BuildSpan($"delete {this.GetType().Name.ToLower()}", LogKeys.DomainRepository).Activate())
            {
                return await this.decoratee.DeleteAsync(id).AnyContext();
            }
        }

        public async Task<ActionResult> DeleteAsync(TEntity entity)
        {
            using(var scope = this.tracer?.BuildSpan($"delete {this.GetType().Name.ToLower()}", LogKeys.DomainRepository).Activate())
            {
                return await this.decoratee.DeleteAsync(entity).AnyContext();
            }
        }

        public async Task<bool> ExistsAsync(object id)
        {
            using(var scope = this.tracer?.BuildSpan($"exists {this.GetType().Name.ToLower()}", LogKeys.DomainRepository).Activate())
            {
                return await this.decoratee.ExistsAsync(id).AnyContext();
            }
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            using(var scope = this.tracer?.BuildSpan($"findall {this.GetType().Name.ToLower()}", LogKeys.DomainRepository).Activate())
            {
                return await this.decoratee.FindAllAsync(options, cancellationToken).AnyContext();
            }
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            using(var scope = this.tracer?.BuildSpan($"findall {this.GetType().Name.ToLower()}", LogKeys.DomainRepository).Activate())
            {
                return await this.decoratee.FindAllAsync(specification, options, cancellationToken).AnyContext();
            }
        }

        public async Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default)
        {
            using(var scope = this.tracer?.BuildSpan($"findall {this.GetType().Name.ToLower()}", LogKeys.DomainRepository).Activate())
            {
                return await this.decoratee.FindAllAsync(specifications, options, cancellationToken).AnyContext();
            }
        }

        public async Task<TEntity> FindOneAsync(object id)
        {
            using(var scope = this.tracer?.BuildSpan($"findone {this.GetType().Name.ToLower()}", LogKeys.DomainRepository).Activate())
            {
                return await this.decoratee.FindOneAsync(id).AnyContext();
            }
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            using(var scope = this.tracer?.BuildSpan($"insert {this.GetType().Name.ToLower()}", LogKeys.DomainRepository).Activate())
            {
                return await this.decoratee.InsertAsync(entity).AnyContext();
            }
        }

        public async Task<TEntity> UpdateAsync(TEntity entity)
        {
            using(var scope = this.tracer?.BuildSpan($"update {this.GetType().Name.ToLower()}", LogKeys.DomainRepository).Activate())
            {
                return await this.decoratee.UpdateAsync(entity).AnyContext();
            }
        }

        public async Task<(TEntity entity, ActionResult action)> UpsertAsync(TEntity entity)
        {
            using(var scope = this.tracer?.BuildSpan($"upsert {this.GetType().Name.ToLower()}", LogKeys.DomainRepository).Activate())
            {
                return await this.decoratee.UpsertAsync(entity).AnyContext();
            }
        }
    }
}
