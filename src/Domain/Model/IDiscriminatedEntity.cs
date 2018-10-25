namespace Naos.Core.Domain
{
    public interface IDiscriminatedEntity
    {
        /// <summary>
        /// Gets the type of the entity (discriminator).
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        string Discriminator { get; }
    }
}
