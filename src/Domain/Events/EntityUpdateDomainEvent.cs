namespace Naos.Core.Domain
{
    public class EntityUpdateDomainEvent : DomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityUpdateDomainEvent"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public EntityUpdateDomainEvent(IEntity entity)
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
