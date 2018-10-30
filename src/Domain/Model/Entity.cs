namespace Naos.Core.Domain
{
    using Naos.Core.Common;
    using Newtonsoft.Json;

    /// <summary>
    /// A base class for all domain entities (layer supertype)
    /// </summary>
    /// <typeparam name="TId">The type of the identifier.</typeparam>
    public abstract class Entity<TId> : IEntity<TId>, IStateEntity, IDiscriminatedEntity
    {
        /// <summary>
        /// Gets or sets the entity id.
        /// </summary>
        /// <remarks>
        ///     <para>
        ///         The id may be of type <c>string</c>, <c>int</c>, or another value type.
        ///     </para>
        /// </remarks>
        public TId Id { get; set; }

        /// <summary>
        /// Gets the identifier value.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [JsonProperty(PropertyName = "id")]
        object IEntity.Id
        {
            get { return this.Id; }
        }

        /// <summary>
        /// Gets the type of the entity (discriminator).
        /// </summary>
        /// <value>
        /// The type of the entity.
        /// </value>
        //[JsonProperty(PropertyName = "_et")]
        public string Discriminator => this.GetType().FullName;

        /// <summary>
        /// Gets the state for this instance.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        public EntityState State { get; private set; } = new EntityState();

#pragma warning disable S3875 // "operator==" should not be overloaded on reference types
        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">The first object instance</param>
        /// <param name="b">The second object instance</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Entity<TId> a, Entity<TId> b)
#pragma warning restore S3875 // "operator==" should not be overloaded on reference types
        {
            if (ReferenceEquals(a, null) && ReferenceEquals(b, null))
            {
                return true;
            }

            if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
            {
                return false;
            }

            return a.Id.Equals(b.Id);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">The first object instance</param>
        /// <param name="b">The second object instance</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Entity<TId> a, Entity<TId> b) => !(a == b);

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = obj as Entity<TId>;

            if (ReferenceEquals(other, null))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            if (this.GetType() != other.GetType())
            {
                return false;
            }

            return this.Id.Equals(other.Id);
            //return base.Equals(obj);
            //return this.Id.IsNullOrEmpty() || other.Id.IsNullOrEmpty() ? false : this.Id == other.Id;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => this.GetType().GetHashCode() ^ this.Id.GetHashCode();

        /// <summary>
        /// Determines whether this instance is transient (not persisted).
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is transient; otherwise, <c>false</c>.
        /// </returns>
        public bool IsTransient() => this.Id.IsDefault();

        /// <summary>
        /// Updates the hash code for the entity.
        /// </summary>
        public void UpdateIdentifierHash()
        {
            (this.State ?? (this.State = new EntityState())).UpdateIdentifierHash(this);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString() => $"{this.Discriminator} [Id={this.Id}]";
    }
}
