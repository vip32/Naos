namespace Naos.Core.Domain
{
    using System;
    using System.Linq.Expressions;

    public interface ISpecification<T>
    {
        Expression<Func<T, bool>> Expression();

        Func<T, bool> ToPredicate();

        bool IsSatisfiedBy(T entity);

        bool IsNotSatisfiedBy(T entity);

        ISpecification<T> Or(ISpecification<T> specification);

        ISpecification<T> And(ISpecification<T> specification);
    }
}