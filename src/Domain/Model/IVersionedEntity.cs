namespace Naos.Core.Domain
{
    public interface IVersionedEntity
    {
        /// <summary>
        /// Gets the identifier for a specific version. Can be used as an ETag
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        string VersionIdentifier { get; }

        /// <summary>
        /// Updates the version identifier to the current instance state
        /// </summary>
        void UpdateVersionIdentifier();
    }
}
