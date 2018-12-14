namespace Naos.Core.Domain
{
    public class EntityUpdatedDomainEvent : IDomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityUpdatedDomainEvent"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public EntityUpdatedDomainEvent(IEntity entity)
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
