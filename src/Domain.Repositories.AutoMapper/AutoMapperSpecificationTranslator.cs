namespace Naos.Core.Domain.Repositories.AutoMapper
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;
    using global::AutoMapper;
    using global::AutoMapper.Extensions.ExpressionMapping;
    using Naos.Core.Domain.Specifications;

    public class AutoMapperSpecificationTranslator<TEntity, TDestination> : ISpecificationTranslator<TEntity, TDestination>
    {
        private readonly IMapper mapper;

        public AutoMapperSpecificationTranslator(IMapper mapper)
        {
            EnsureArg.IsNotNull(mapper, typeof(mapper));

            this.mapper = mapper;
        }

        public bool CanHandle(ISpecification<TEntity> specification)
        {
            return true;
        }

        public Func<TDestination, bool> Translate(ISpecification<TEntity> specification)
        {
            var expression = this.mapper
                .MapExpression<Expression<Func<TDestination, bool>>>(specification.ToExpression());
            return expression.Compile();
        }
    }
}
