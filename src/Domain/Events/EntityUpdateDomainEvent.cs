namespace Naos.Core.Domain
{
    public class EntityUpdateDomainEvent<TEntity> : IDomainEvent
        where TEntity : class, IEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityUpdateDomainEvent{TEntity}"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public EntityUpdateDomainEvent(TEntity entity)
        {
            this.Entity = entity;
        }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public TEntity Entity { get; set; }
    }
}
