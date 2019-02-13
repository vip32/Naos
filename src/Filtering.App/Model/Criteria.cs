namespace Naos.Core.RequestFiltering.App
{
    using System;
    using System.Linq;
    using System.Linq.Dynamic.Core;
    using System.Linq.Expressions;
    using EnsureThat;

    public class Criteria
    {
        public Criteria(
            string name,
            CriteriaOperator @operator,
            object value,
            bool? isNumeric = null)
        {
            EnsureArg.IsNotNullOrEmpty(name);

            this.Name = name;
            this.Operator = @operator;
            this.Value = value ?? new object();
            this.IsNumeric = isNumeric ?? value.ToString().All(char.IsDigit);
        }

        public string Name { get; }

        public CriteriaOperator Operator { get; }

        public object Value { get; }

        public bool IsNumeric { get; }

        public Expression<Func<T, bool>> ToExpression<T>()
        {
            return DynamicExpressionParser
                .ParseLambda<T, bool>(ParsingConfig.Default, false, this.ToString());

            // jokenizer
            //return Evaluator.ToLambda<T, bool>(Tokenizer.Parse(this.ToString()));
        }

        public override string ToString()
        {
            var quote = this.IsNumeric ? string.Empty : "\"";
            if (this.Operator.IsFunction())
            {
                // function based operator
                return $"(it.{this.Name}.{this.Operator.ToValue()}({quote}{this.Value}{quote}))";
            }
            else
            {
                // standard operators
                return $"(it.{this.Name}{this.Operator.ToValue()}{quote}{this.Value}{quote})";
            }
        }

        // jokenizer
        //public override string ToString()
        //{
        //    var quote = this.IsNumeric ? string.Empty : "\"";
        //    if (this.Operator.IsFunction())
        //    {
        //        // function based operator
        //        return $"(t) => t.{this.Name}.{this.Operator.ToValue()}({quote}{this.Value}{quote})";
        //    }
        //    else
        //    {
        //        // standard operators
        //        return $"(t) => t.{this.Name}{this.Operator.ToValue()}{quote}{this.Value}{quote}";
        //    }
        //}
    }
}
