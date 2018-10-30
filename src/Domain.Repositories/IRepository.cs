namespace Naos.Core.Domain.Repositories
{
    using System.Threading.Tasks;

    public interface IRepository<TEntity> : IReadOnlyRepository<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        //Task<TEntity> InsertAsync(TEntity entity);

        //Task<TEntity> UpdateAsync(TEntity entity);

        Task<TEntity> UpsertAsync(TEntity entity);

        Task DeleteAsync(object id);

        Task DeleteAsync(TEntity entity);
    }
}