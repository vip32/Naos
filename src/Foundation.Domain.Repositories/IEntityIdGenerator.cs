namespace Naos.Foundation.Domain
{
    public interface IEntityIdGenerator<TEntity>
        where TEntity : class, IEntity, IAggregateRoot
    {
        void SetNew(TEntity entity);
    }
}