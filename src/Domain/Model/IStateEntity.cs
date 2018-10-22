namespace Naos.Core.Domain
{
    public interface IStateEntity
    {
        /// <summary>
        /// Gets the state for an entity.
        /// </summary>
        /// <value>
        /// The current state.
        /// </value>
        EntityState State { get; }
    }
}
