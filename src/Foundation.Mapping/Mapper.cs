namespace Naos.Foundation
{
    using System;

    /// <summary>
    /// Maps an object of type <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the object to map from.</typeparam>
    /// <typeparam name="TDestination">The type of the object to map to.</typeparam>
    public class Mapper<TSource, TDestination> : IMapper<TSource, TDestination> // TODO: rename to ActionMapper?
    {
        private readonly Action<TSource, TDestination> action;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mapper{TSource, TDestination}"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        public Mapper(Action<TSource, TDestination> action)
        {
            this.action = action;
        }

        /// <summary>
        /// Maps the specified source object into the destination object.
        /// </summary>
        /// <param name="source">The source object to map from.</param>
        /// <param name="destination">The destination object to map to.</param>
        public void Map(TSource source, TDestination destination)
        {
            if(source != null && destination != null)
            {
                this.action(source, destination);
            }
        }
    }
}
