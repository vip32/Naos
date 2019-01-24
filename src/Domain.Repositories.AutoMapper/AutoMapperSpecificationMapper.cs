namespace Naos.Core.Domain.Repositories.AutoMapper
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;
    using global::AutoMapper;
    using global::AutoMapper.Extensions.ExpressionMapping;
    using Naos.Core.Domain.Specifications;

    public class AutoMapperSpecificationMapper<T, TDestination> : ISpecificationMapper<T, TDestination>
        where T : class
    {
        private readonly IMapper mapper;

        public AutoMapperSpecificationMapper(IMapper mapper)
        {
            EnsureArg.IsNotNull(mapper, nameof(mapper));

            this.mapper = mapper;
        }

        public bool CanHandle(ISpecification<T> specification)
        {
            return true;
        }

        public Func<TDestination, bool> Map(ISpecification<T> specification)
        {
            var expression = this.mapper
                .MapExpression<Expression<Func<TDestination, bool>>>(specification.ToExpression());
            return expression.Compile(); // replace wit CompileFast()? https://github.com/dadhi/FastExpressionCompiler
        }
    }
}
