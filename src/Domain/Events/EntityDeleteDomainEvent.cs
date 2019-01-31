namespace Naos.Core.Domain
{
    public class EntityDeleteDomainEvent : DomainEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDeleteDomainEvent"/> class.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public EntityDeleteDomainEvent(IEntity entity)
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
