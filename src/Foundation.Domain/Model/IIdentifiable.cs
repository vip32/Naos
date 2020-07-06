namespace Naos.Foundation.Domain
{

    public interface IIdentifiable
    {
        string IdentifierHash { get; }

        /// <summary>
        /// Updates the state hash for an entity.
        /// </summary>
        void SetIdentifierHash();
    }
}
