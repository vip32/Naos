namespace Naos.Core.Common
{
    using EnsureThat;

    public class AutoMapper<TSource, TDestination> : IMapper<TSource, TDestination>
    {
        // TODO: an alternative mapper is FluentInjectorEntityMapper, or hand coded
        private readonly global::AutoMapper.IMapper mapper;

        public AutoMapper(global::AutoMapper.IMapper mapper)
        {
            EnsureArg.IsNotNull(mapper, nameof(mapper));

            this.mapper = mapper;
        }

        public void Map(TSource source, TDestination destination)
        {
            if(source != null && destination != null)
            {
                this.mapper.Map(source, destination);
            }
        }
    }
}
