namespace Naos.Core.Domain
{
    public class EntityUpdatedDomainEvent<TEntity> : IDomainEvent
        where TEntity : class, IEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityUpdatedDomainEvent{TEntity}"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public EntityUpdatedDomainEvent(TEntity entity)
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
