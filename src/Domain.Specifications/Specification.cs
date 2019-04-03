namespace Naos.Core.Domain.Specifications
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;
    using Naos.Core.Common;

    public /*abstract*/ class Specification<T> : ISpecification<T>
    {
        public static readonly ISpecification<T> All = new IdentitySpecification<T>();
        private readonly Expression<Func<T, bool>> expression;

        public Specification()
        {
        }

        public Specification(Expression<Func<T, bool>> expression)
        {
            this.expression = expression;
        }

        public Specification(string expression)
        {
            EnsureArg.IsNotNullOrEmpty(expression);

            this.expression = ExpressionHelper.FromExpressionString<T>(expression);

            // jokenizer
            //this.expression = Evaluator.ToLambda<T, bool>(
            //    Tokenizer.Parse($"(t) => t.{expression}"));
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
            if(entity == default)
            {
                return false;
            }

            var predicate = this.ToPredicate();
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
