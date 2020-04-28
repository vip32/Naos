namespace Naos.Foundation.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public interface ISpecification
    {
        string Name { get; set; }

        string Description { get; set; }

        Expression<Func<bool>> ToExpression();

        Func<bool> ToPredicate();

        bool IsSatisfied();

        ISpecification Or(ISpecification specification);

        ISpecification Or(IEnumerable<ISpecification> specifications);

        ISpecification And(ISpecification specification);

        ISpecification And(IEnumerable<ISpecification> specifications);

        ISpecification Not();

        string ToString();

        string ToString(bool noBrackets);
    }
}