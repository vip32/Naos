namespace Naos.Foundation.Domain
{
    public interface IHaveDiscriminator // TODO: obsolete because of cosmos client v3
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
