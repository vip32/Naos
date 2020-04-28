namespace Naos.Foundation.Domain
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using EnsureThat;

    public class NotSpecification : Specification
    {
        private readonly ISpecification specification;

        public NotSpecification(ISpecification specification)
        {
            EnsureArg.IsNotNull(specification);

            this.specification = specification;
        }

        public override Expression<Func<bool>> ToExpression()
        {
            var expression = this.specification.ToExpression();

            var notepression = Expression.Not(expression.Body);
            return Expression.Lambda<Func<bool>>(notepression, expression.Parameters.Single());
        }

        public override string ToString()
        {
            return this.ToExpression()?.ToString();
        }
    }
}
