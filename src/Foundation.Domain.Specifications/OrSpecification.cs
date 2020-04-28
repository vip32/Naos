namespace Naos.Foundation.Domain
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using EnsureThat;

    public class OrSpecification : Specification
    {
        private readonly ISpecification leftSpecification;
        private readonly ISpecification rightSpecification;

        public OrSpecification(ISpecification leftSpecification, ISpecification rightSpecification)
        {
            EnsureArg.IsNotNull(leftSpecification);
            EnsureArg.IsNotNull(rightSpecification);

            this.rightSpecification = rightSpecification;
            this.leftSpecification = leftSpecification;
        }

        public override Expression<Func<bool>> ToExpression()
        {
            var leftExpression = this.leftSpecification.ToExpression();
            var rightExpression = this.rightSpecification.ToExpression();

            //var orExpression = Expression.OrElse(leftExpression.Body, rightExpression.Body);
            var orExpression = Expression.OrElse(
                leftExpression.Body,
                Expression.Invoke(rightExpression, leftExpression.Parameters.Single()));

            //return Expression.Lambda<Func<T, bool>>(orExpression, leftExpression.Parameters.Single());
            return Expression.Lambda<Func<bool>>(orExpression, leftExpression.Parameters);
        }

        public override string ToString()
        {
            return this.ToExpression()?.ToString();
        }
    }
}
