namespace Naos.Core.Domain.Specifications
{
    using System;
    using System.Linq.Expressions;

    public abstract class Specification<T> : ISpecification<T>
    {
        public static readonly ISpecification<T> All = new IdentitySpecification<T>();

        public abstract Expression<Func<T, bool>> ToExpression();

        public Func<T, bool> ToPredicate()
        {
            return this.ToExpression().Compile();
        }

        //public Predicate<T> Predicate()
        //{
        //    var func = this.Expression().Compile();
        //    return t => func(t);
        //}

        public bool IsSatisfiedBy(T entity)
        {
            if(entity == default)
            {
                return false;
            }

            Func<T, bool> predicate = this.ToPredicate();
            return predicate(entity);
        }

        public ISpecification<T> And(ISpecification<T> specification)
        {
            if(this == All)
            {
                return specification;
            }

            if (specification == All)
            {
                return this;
            }

            return new AndSpecification<T>(this, specification);
        }

        public ISpecification<T> Or(ISpecification<T> specification)
        {
            if(this == All || specification == All)
            {
                return All;
            }

            return new OrSpecification<T>(this, specification);
        }

        public ISpecification<T> Not()
        {
            return new NotSpecification<T>(this);
        }
    }
}
