namespace Naos.Foundation
{
    using EnsureThat;

    /// <summary>
    /// Maps an object of type <typeparamref name="TSource"/> to <typeparamref name="TDestination"/>.
    /// </summary>
    /// <typeparam name="TSource">The type of the object to map from.</typeparam>
    /// <typeparam name="TDestination">The type of the object to map to.</typeparam>
    public class AutoMapper<TSource, TDestination> : IMapper<TSource, TDestination>
    {
        // TODO: an alternative mapper is FluentInjectorEntityMapper, or hand coded
        private readonly global::AutoMapper.IMapper mapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoMapper{TSource, TDestination}"/> class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        public AutoMapper(global::AutoMapper.IMapper mapper)
        {
            EnsureArg.IsNotNull(mapper, nameof(mapper));

            this.mapper = mapper;
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
                this.mapper.Map(source, destination);
            }
        }
    }
}
