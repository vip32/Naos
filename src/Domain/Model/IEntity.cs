namespace Naos.Core.Domain
{
    public interface IEntity
    {
        /// <summary>
        /// Gets the identifier value.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        object Id { get; set; }
    }

    public interface IEntity<TId> : IEntity
    {
        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The id may be of type <c>string</c>, <c>int</c>, or another value type.
        ///     </para>
        /// </remarks>
        new TId Id { get; set; }

        /// <summary>
        /// Determines whether this instance is transient (not persisted).
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is transient; otherwise, <c>false</c>.
        /// </returns>
        //bool IsTransient();
    }
}
