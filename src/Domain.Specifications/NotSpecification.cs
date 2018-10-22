namespace Naos.Core.Domain.Specifications
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using EnsureThat;

    public class NotSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> specification;

        public NotSpecification(ISpecification<T> specification)
        {
            EnsureArg.IsNotNull(specification);

            this.specification = specification;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            Expression<Func<T, bool>> expression = this.specification.ToExpression();

            var notepression = Expression.Not(expression.Body);
            return Expression.Lambda<Func<T, bool>>(notepression, expression.Parameters.Single());
        }
    }
}
