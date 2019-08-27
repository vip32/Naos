namespace Naos.Foundation
{
    using System.Collections.Generic;
    using EnsureThat;

    /// <summary>
    /// <see cref="IMapper{TSource, TTarget}"/> extension methods.
    /// </summary>
    public static class MapperExtensions
    {
        /// <summary>
        /// Maps the specified source object to a new object with a type of <typeparamref name="TTarget"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <param name="mapper">The mapper instance.</param>
        /// <param name="source">The source object.</param>
        /// <returns>The mapped object of type <typeparamref name="TTarget"/>.</returns>
        public static TTarget Map<TSource, TTarget>(
            this IMapper<TSource, TTarget> mapper,
            TSource source)
            where TTarget : class, new()
        {
            EnsureArg.IsNotNull(mapper, nameof(mapper));

            if (source == null)
            {
                return null;
            }

            var target = Factory<TTarget>.Create();
            mapper.Map(source, target);
            return target;
        }

        /// <summary>
        /// Maps the specified source object to a new object with a type of <typeparamref name="TTarget"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the source object.</typeparam>
        /// <typeparam name="TTarget">The type of the target object.</typeparam>
        /// <param name="mapper">The mapper instance.</param>
        /// <param name="source">The source object.</param>
        /// <param name="safe">Handles null sources.</param>
        /// <returns>The mapped object of type <typeparamref name="TTarget"/>.</returns>
        public static TTarget Map<TSource, TTarget>(
            this IMapper<TSource, TTarget> mapper,
            TSource source,
            bool safe)
            where TSource : class, new()
            where TTarget : class, new()
        {
            EnsureArg.IsNotNull(mapper, nameof(mapper));

            if (source == null && !safe)
            {
                return null;
            }
            else if (source == null)
            {
                source = Factory<TSource>.Create();
            }

            var target = Factory<TTarget>.Create();
            mapper.Map(source, target);
            return target;
        }

        /// <summary>
        /// Maps the collection of <typeparamref name="TSource"/> into a IEnumerable of /// <typeparamref name="TTarget"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the source objects.</typeparam>
        /// <typeparam name="TTarget">The type of the target objects.</typeparam>
        /// <param name="mapper">The mapper instance.</param>
        /// <param name="sources">The source collection.</param>
        /// <returns>An array of <typeparamref name="TTarget"/>.</returns>
        public static IEnumerable<TTarget> Map<TSource, TTarget>(
            this IMapper<TSource, TTarget> mapper,
            IEnumerable<TSource> sources)
            where TTarget : class, new()
        {
            EnsureArg.IsNotNull(mapper, nameof(mapper));

            foreach (var source in sources.Safe())
            {
                yield return mapper.Map(source);
            }
        }

        /// <summary>
        /// Maps the array of <typeparamref name="TSource"/> into an IEnumerable of /// <typeparamref name="TTarget"/>.
        /// </summary>
        /// <typeparam name="TSource">The type of the source objects.</typeparam>
        /// <typeparam name="TTarget">The type of the target objects.</typeparam>
        /// <param name="mapper">The mapper instance.</param>
        /// <param name="sources">The source objects.</param>
        /// <returns>An array of <typeparamref name="TTarget"/>.</returns>
        public static IEnumerable<TTarget> Map<TSource, TTarget>(
            this IMapper<TSource, TTarget> mapper,
            TSource[] sources)
            where TTarget : class, new()
        {
            EnsureArg.IsNotNull(mapper, nameof(mapper));

            for (var i = 0; i < sources.Safe().Count; ++i)
            {
                var source = sources[i];
                var target = Factory<TTarget>.Create();
                mapper.Map(source, target);
                yield return target;
            }
        }
    }
}