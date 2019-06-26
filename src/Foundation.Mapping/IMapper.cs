namespace Naos.Foundation
{
    /// <summary>
    /// Maps an object of type <typeparamref name="TSource"/> to <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the object to map from.</typeparam>
    /// <typeparam name="TTarget">The type of the object to map to.</typeparam>
    public interface IMapper<TSource, TTarget>
    {
        /// <summary>
        /// Maps the specified source object into the destination object.
        /// </summary>
        /// <param name="source">The source object to map from.</param>
        /// <param name="target">The target object to map to.</param>
        void Map(TSource source, TTarget target);
    }
}
