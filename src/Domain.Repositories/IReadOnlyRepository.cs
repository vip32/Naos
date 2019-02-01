namespace Naos.Core.Domain.Repositories
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Naos.Core.Domain.Specifications;

    public interface IReadOnlyRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        Task<IEnumerable<TEntity>> FindAllAsync(IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>> FindAllAsync(ISpecification<TEntity> specification, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default);

        Task<IEnumerable<TEntity>> FindAllAsync(IEnumerable<ISpecification<TEntity>> specifications, IFindOptions<TEntity> options = null, CancellationToken cancellationToken = default);

        Task<TEntity> FindOneAsync(object id);

        //Task<T> FindOneAsync(ISpecification<T> specification);

        Task<bool> ExistsAsync(object id);
    }
}