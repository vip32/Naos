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
}
