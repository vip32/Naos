namespace Naos.Core.Domain.Repositories.AutoMapper
{
    using System.Linq.Expressions;
    using global::AutoMapper.Extensions.ExpressionMapping;
    using Naos.Core.Common.Mapping;

    public class AutoMapperEntityMapper : IMapper
    {
        // TODO: an alternative mapper is FluentInjectorEntityMapper, or hand coded
        private readonly global::AutoMapper.IMapper mapper;

        public AutoMapperEntityMapper(global::AutoMapper.IMapper mapper)
        {
            EnsureThat.EnsureArg.IsNotNull(mapper, nameof(mapper));

            this.mapper = mapper;
        }

        public TDestination Map<TDestination>(object source)
        {
            return this.mapper.Map<TDestination>(source);
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            return this.mapper.Map<TDestination>(source);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return this.mapper.Map(source, destination);
        }

        public TDestination MapExpression<TDestination>(LambdaExpression expression)
            where TDestination : LambdaExpression
        {
            return this.mapper.MapExpression<TDestination>(expression);
        }
    }
}
