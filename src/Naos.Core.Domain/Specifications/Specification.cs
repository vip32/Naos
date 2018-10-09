namespace Naos.Core.Domain
{
    using System;
    using System.Linq.Expressions;

    public abstract class Specification<T> : ISpecification<T>
    {
        public abstract Expression<Func<T, bool>> Expression();

        public Func<T, bool> ToPredicate()
        {
            return this.Expression().Compile();
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

        public bool IsNotSatisfiedBy(T entity)
        {
            if (entity == default)
            {
                return true;
            }

            Func<T, bool> predicate = this.ToPredicate();
            return !predicate(entity);
        }

        public ISpecification<T> And(ISpecification<T> specification)
        {
            return new AndSpecification<T>(this, specification);
        }

        public ISpecification<T> Or(ISpecification<T> specification)
        {
            return new OrSpecification<T>(this, specification);
        }
    }
}
