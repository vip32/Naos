namespace Naos.Core.Domain
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IRepository<T>
        where T : class, IEntity, IAggregateRoot
    {
        Task<IEnumerable<T>> FindAllAsync(IFindOptions options = null);

        Task<IEnumerable<T>> FindAllAsync(ISpecification<T> specification, IFindOptions options = null);

        Task<IEnumerable<T>> FindAllAsync(IEnumerable<ISpecification<T>> specifications, IFindOptions options = null);

        Task<T> FindOneAsync(object id);

        //Task<T> FindOneAsync(ISpecification<T> specification);

        Task<bool> ExistsAsync(object id);

        Task<T> AddOrUpdateAsync(T entity);

        Task DeleteAsync(object id);

        Task DeleteAsync(T entity);
    }
}