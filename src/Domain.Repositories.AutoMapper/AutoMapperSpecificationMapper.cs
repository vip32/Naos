namespace Naos.Core.Domain.Repositories.AutoMapper
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;
    using global::AutoMapper;
    using global::AutoMapper.Extensions.ExpressionMapping;
    using Naos.Core.Domain.Specifications;

    public class AutoMapperSpecificationMapper<TEntity, TDestination>
        : ISpecificationMapper<TEntity, TDestination>
    {
        private readonly IMapper mapper;

        public AutoMapperSpecificationMapper(IMapper mapper)
        {
            EnsureArg.IsNotNull(mapper, nameof(mapper));

            this.mapper = mapper;
        }

        public bool CanHandle(ISpecification<TEntity> specification)
        {
            return true;
        }

        public Func<TDestination, bool> Map(ISpecification<TEntity> specification)
        {
            var expression = this.mapper
                .MapExpression<Expression<Func<TDestination, bool>>>(specification.ToExpression());
            return expression.Compile();
        }
    }
}
