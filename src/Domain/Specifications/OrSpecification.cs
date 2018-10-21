namespace Naos.Core.Domain
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using EnsureThat;

    public class OrSpecification<T> : Specification<T>
    {
        private readonly ISpecification<T> leftSpecification;
        private readonly ISpecification<T> rightSpecification;

        public OrSpecification(ISpecification<T> leftSpecification, ISpecification<T> rightSpecification)
        {
            EnsureArg.IsNotNull(leftSpecification);
            EnsureArg.IsNotNull(rightSpecification);

            this.rightSpecification = rightSpecification;
            this.leftSpecification = leftSpecification;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            Expression<Func<T, bool>> leftExpression = this.leftSpecification.ToExpression();
            Expression<Func<T, bool>> rightExpression = this.rightSpecification.ToExpression();

            //var orExpression = Expression.OrElse(leftExpression.Body, rightExpression.Body);
            var orExpression = Expression.OrElse(
                leftExpression.Body,
                Expression.Invoke(rightExpression, leftExpression.Parameters.Single()));

            //return Expression.Lambda<Func<T, bool>>(orExpression, leftExpression.Parameters.Single());
            return Expression.Lambda<Func<T, bool>>(orExpression, leftExpression.Parameters);
        }
    }
}
