namespace Naos.Core.Common
{
    using System.Collections.Generic;
    using EnsureThat;

    /// <summary>
    /// <see cref="IMapper{TSource, TDestination}"/> extension methods.
    /// </summary>
    public static class MapperExtensions
    {
        /// <summary>
        /// Maps the specified source object to a new object with a type of <typeparamref name="TDestination"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TDestination">The type of the destination object.</typeparam>
        /// <param name="mapper">The mapper instance.</param>
        /// <param name="source">The source object.</param>
        /// <returns>The mapped object of type <typeparamref name="TDestination"/>.</returns>
        public static TDestination Map<TSource, TDestination>(
            this IMapper<TSource, TDestination> mapper,
            TSource source)
            where TDestination : class, new()
        {
            EnsureArg.IsNotNull(mapper, nameof(mapper));

            if (source == null)
            {
                return null;
            }

            var destination = Factory<TDestination>.Create();
            mapper.Map(source, destination);
            return destination;
        }

        /// <summary>
        /// Maps the collection of <typeparamref name="TSource"/> into a IEnumerable of /// <typeparamref name="TDestination"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the source objects.</typeparam>
        /// <typeparam name="TDestination">The type of the destination objects.</typeparam>
        /// <param name="mapper">The mapper instance.</param>
        /// <param name="sources">The source collection.</param>
        /// <returns>An array of <typeparamref name="TDestination"/>.</returns>
        public static IEnumerable<TDestination> Map<TSource, TDestination>(
            this IMapper<TSource, TDestination> mapper,
             IEnumerable<TSource> sources)
            where TDestination : class, new()
        {
            EnsureArg.IsNotNull(mapper, nameof(mapper));

            foreach (var source in sources.Safe())
            {
                yield return mapper.Map(source);
            }
        }

        /// <summary>
        /// Maps the array of <typeparamref name="TSource"/> into an IEnumerable of /// <typeparamref name="TDestination"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the source objects.</typeparam>
        /// <typeparam name="TDestination">The type of the destination objects.</typeparam>
        /// <param name="mapper">The mapper instance.</param>
        /// <param name="sources">The source objects.</param>
        /// <returns>An array of <typeparamref name="TDestination"/>.</returns>
        public static IEnumerable<TDestination> Map<TSource, TDestination>(
            this IMapper<TSource, TDestination> mapper,
            TSource[] sources)
            where TDestination : class, new()
        {
            EnsureArg.IsNotNull(mapper, nameof(mapper));

            for (var i = 0; i < sources.Safe().Count; ++i)
            {
                var source = sources[i];
                var destinationItem = Factory<TDestination>.Create();
                mapper.Map(source, destinationItem);
                yield return destinationItem;
            }
        }
    }
}