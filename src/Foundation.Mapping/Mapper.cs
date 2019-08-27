namespace Naos.Foundation
{
    using System;

    /// <summary>
    /// Maps an object of type <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the object to map from.</typeparam>
    /// <typeparam name="TTarget">The type of the object to map to.</typeparam>
    public class Mapper<TSource, TTarget> : IMapper<TSource, TTarget> // TODO: rename to ActionMapper?
    {
        private readonly Action<TSource, TTarget> action;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mapper{TSource, TDestination}"/> class.
        /// </summary>
        /// <param name="action">The action.</param>
        public Mapper(Action<TSource, TTarget> action)
        {
            this.action = action;
        }

        /// <summary>
        /// Maps the specified source object into the destination object.
        /// </summary>
        /// <param name="source">The source object to map from.</param>
        /// <param name="target">The target object to map to.</param>
        public void Map(TSource source, TTarget target)
        {
            if (source != null && target != null)
            {
                this.action(source, target);
            }
        }
    }
}
