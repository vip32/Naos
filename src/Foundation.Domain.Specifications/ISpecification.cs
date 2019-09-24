namespace Naos.Foundation.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface ISpecification<T>
    {
        string Name { get; set; }

        Expression<Func<T, bool>> ToExpression();

        Func<T, bool> ToPredicate();

        bool IsSatisfiedBy(T entity);

        ISpecification<T> Or(ISpecification<T> specification);

        ISpecification<T> Or(IEnumerable<ISpecification<T>> specifications);

        ISpecification<T> And(ISpecification<T> specification);

        ISpecification<T> And(IEnumerable<ISpecification<T>> specifications);

        ISpecification<T> Not();

        string ToString();

        string ToString(bool noBrackets);
    }
}