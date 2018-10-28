namespace Naos.Core.Domain.Repositories.AutoMapper
{
    using System.Linq.Expressions;
    using global::AutoMapper;
    using global::AutoMapper.Extensions.ExpressionMapping;
    using Naos.Core.Domain.Repositories;

    public class AutoMapperEntityMapper : IEntityMapper
    {
        // TODO: an alternative mapper is FluentInjectorEntityMapper, or hand coded
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

        public TDestination MapExpression<TDestination>(LambdaExpression source)
            where TDestination : LambdaExpression
        {
            return this.mapper.MapExpression<TDestination>(source);
        }
    }
}
