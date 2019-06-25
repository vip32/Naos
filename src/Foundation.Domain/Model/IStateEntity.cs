namespace Naos.Foundation.Domain
{
    public interface IStateEntity
    {
        /// <summary>
        /// Gets the state for an entity.
        /// </summary>
        /// <value>
        /// The current state.
        /// </value>
        State State { get; }
    }

    public interface IIdentifiable
    {
        string IdentifierHash { get; }

        /// <summary>
        /// Updates the state hash for an entity.
        /// </summary>
        void SetIdentifierHash();
    }
}
