namespace Naos.Foundation.Domain
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;
    using global::AutoMapper.Extensions.ExpressionMapping;

    public class AutoMapperEntityMapper : IEntityMapper
    {
        // TODO: an alternative mapper is FluentInjectorEntityMapper, or hand coded
        private readonly global::AutoMapper.IMapper mapper;

        public AutoMapperEntityMapper(global::AutoMapper.IMapper mapper)
        {
            EnsureArg.IsNotNull(mapper, nameof(mapper));

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

        public Func<TDestination, bool> MapAsPredicate<TSource, TDestination>(ISpecification<TSource> specification)
        {
            EnsureArg.IsNotNull(specification, nameof(specification));

            var expression = this.mapper
                .MapExpression<Expression<Func<TDestination, bool>>>(specification.ToExpression());
            return expression.Compile(); // replace wit CompileFast()? https://github.com/dadhi/FastExpressionCompiler
        }

        public Expression<Func<TDestination, bool>> MapAsExpression<TSource, TDestination>(ISpecification<TSource> specification)
        {
            EnsureArg.IsNotNull(specification, nameof(specification));

            return this.mapper
                .MapExpression<Expression<Func<TDestination, bool>>>(specification.ToExpression()); // replace wit CompileFast()? https://github.com/dadhi/FastExpressionCompiler
        }
    }
}
