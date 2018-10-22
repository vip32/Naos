namespace Naos.Core.Domain.Repositories.AutoMapper
{
    using global::AutoMapper;
    using Naos.Core.Domain.Repositories;

    public class AutoMapperEntityMapper : IEntityMapper
    {
        private readonly IMapper mapper;

        public AutoMapperEntityMapper(IMapper mapper)
        {
            EnsureThat.EnsureArg.IsNotNull(mapper, nameof(mapper));

            this.mapper = mapper;
        }

        public TDestination Map<TDestination>(object source)
        {
            return this.mapper.Map<TDestination>(source);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return this.mapper.Map(source, destination);
        }
    }
}
