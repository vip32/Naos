namespace Naos.Core.Domain
{
    public interface IHaveDiscriminator // TODO: obsolete when seperate cosmos collections are used (cosmosrepo V3)
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
