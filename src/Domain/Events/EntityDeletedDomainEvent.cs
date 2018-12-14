namespace Naos.Core.Domain
{
    public class EntityDeletedDomainEvent : IDomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDeletedDomainEvent"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public EntityDeletedDomainEvent(IEntity entity)
        {
            this.Entity = entity;
        }

        /// <summary>
        /// Gets or sets the entity.
        /// </summary>
        /// <value>
        /// The entity.
        /// </value>
        public IEntity Entity { get; set; }
    }
}
