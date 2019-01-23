namespace Naos.Core.Filtering.App
{
    using System;
    using System.Linq.Expressions;
    using EnsureThat;
    using Jokenizer.Net;

    public class Criteria
    {
        public Criteria(
            string name,
            CriteriaOperator @operator,
            object value,
            bool isNumeric = false)
        {
            EnsureArg.IsNotNullOrEmpty(name);

            this.Name = name;
            this.Operator = @operator;
            this.Value = value;
            this.IsNumeric = isNumeric;
        }

        public string Name { get; }

        public CriteriaOperator Operator { get; }

        public object Value { get; }

        public bool IsNumeric { get; }

        public Expression<Func<T, bool>> ToExpression<T>()
        {
            var quote = this.IsNumeric ? string.Empty : "\"";

            if (this.Operator.IsFunction())
            {
                // function based operator
                return Evaluator.ToLambda<T, bool>(
                    Tokenizer.Parse($"(t) => t.{this.Name}.{this.Operator.ToValue()}({quote}{this.Value}{quote})"));
            }
            else
            {
                // standard operators
                return Evaluator.ToLambda<T, bool>(
                    Tokenizer.Parse($"(t) => t.{this.Name}{this.Operator.ToValue()}{quote}{this.Value}{quote}"));
            }
        }
    }
}
