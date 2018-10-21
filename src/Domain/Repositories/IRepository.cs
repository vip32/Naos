namespace Naos.Core.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository<T>
        where T : class, IEntity, IAggregateRoot
    {
        Task<IEnumerable<T>> FindAllAsync(int? skip = null, int? take = null);

        Task<IEnumerable<T>> FindAllAsync(ISpecification<T> specification, int? skip = null, int? take = null);

        Task<IEnumerable<T>> FindAllAsync(IEnumerable<ISpecification<T>> specifications, int? skip = null, int? take = null);

        Task<T> FindOneAsync(object id);

        //Task<T> FindOneAsync(ISpecification<T> specification);

        Task<bool> ExistsAsync(object id);

        Task<T> AddOrUpdateAsync(T entity);

        Task DeleteAsync(object id);

        Task DeleteAsync(T entity);
    }
}