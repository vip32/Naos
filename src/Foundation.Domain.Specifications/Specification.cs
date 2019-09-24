namespace Naos.Foundation.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using EnsureThat;

    public /*abstract*/ class Specification<T> : ISpecification<T>
    {
        public static readonly ISpecification<T> All = new IdentitySpecification<T>();
        private readonly Expression<Func<T, bool>> expression;

        public Specification()
        {
        }

        public Specification(Expression<Func<T, bool>> expression)
        {
            EnsureArg.IsNotNull(expression);

            this.expression = expression;
            this.Name = ExpressionHelper.GetPropertyName(expression);
        }

        public Specification(string expression)
        {
            EnsureArg.IsNotNullOrEmpty(expression);

            this.expression = ExpressionHelper.FromExpressionString<T>(expression);
            this.Name = ExpressionHelper.GetPropertyName(this.expression);
        }

        public string Name { get; set; }

        public virtual Expression<Func<T, bool>> ToExpression()
        {
            return this.expression;
        }

        public Func<T, bool> ToPredicate()
        {
            return this.ToExpression()?.Compile(); // replace wit CompileFast()? https://github.com/dadhi/FastExpressionCompiler
        }

        public bool IsSatisfiedBy(T entity)
        {
            if (entity == default)
            {
                return false;
            }

            var predicate = this.ToPredicate();
            return predicate(entity);
        }

        public ISpecification<T> And(ISpecification<T> specification)
        {
            if (this == All)
            {
                return specification;
            }

            if (specification == All)
            {
                return this;
            }

            return new AndSpecification<T>(this, specification);
        }

        public ISpecification<T> And(IEnumerable<ISpecification<T>> specifications)
        {
            if (!specifications.IsNullOrEmpty())
            {
                if(specifications.Count() == 1)
                {
                    return this.And(specifications.First());
                }

                var result = specifications.First();

                foreach(var specification in specifications.Skip(1))
                {
                    result = result.And(specification);
                }

                return result;
            }

            return this;
        }

        public ISpecification<T> Or(ISpecification<T> specification)
        {
            if (this == All || specification == All)
            {
                return All;
            }

            return new OrSpecification<T>(this, specification);
        }

        public ISpecification<T> Or(IEnumerable<ISpecification<T>> specifications)
        {
            if (!specifications.IsNullOrEmpty())
            {
                if (specifications.Count() == 1)
                {
                    return this.Or(specifications.First());
                }

                var result = specifications.First();

                foreach (var specification in specifications.Skip(1))
                {
                    result = result.Or(specification);
                }

                return result;
            }

            return this;
        }

        public ISpecification<T> Not()
        {
            return new NotSpecification<T>(this);
        }

        public override string ToString()
        {
            return this.expression.ToExpressionString();
        }

        public string ToString(bool noBrackets)
        {
            if (noBrackets)
            {
                return this.expression.ToExpressionString().Trim('(').Trim(')');
            }

            return this.expression.ToExpressionString();
        }
    }
}
