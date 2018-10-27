namespace Naos.Core.Domain
{
    public class EntityCreateDomainEvent<TEntity> : IDomainEvent
        where TEntity : class, IEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityCreateDomainEvent{TEntity}"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public EntityCreateDomainEvent(TEntity entity)
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
