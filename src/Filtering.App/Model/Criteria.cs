namespace Naos.Core.RequestFiltering.App
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using EnsureThat;
    using Naos.Core.Common;

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
            this.IsNumeric = isNumeric ?? value.ToString().All(char.IsDigit);
            if (this.IsNumeric)
            {
                this.Value = value.To<long>();
            }
            else
            {
                this.Value = value ?? new object();
            }
        }

        public string Name { get; }

        public CriteriaOperator Operator { get; }

        public object Value { get; }

        public bool IsNumeric { get; }

        public Expression<Func<T, bool>> ToExpression<T>()
        {
            return ExpressionHelper.FromExpressionString<T>(this.ToString());

            // jokenizer
            //return Evaluator.ToLambda<T, bool>(Tokenizer.Parse(this.ToString()));
        }

        /// <summary>
        /// Returns a string representation for this specification
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var quote = this.IsNumeric ? string.Empty : "\"";
            if (this.Operator.IsFunction())
            {
                // function based operator
                return $"({this.Name}.{this.Operator.ToValue()}({quote}{this.Value}{quote}))";
            }
            else
            {
                // standard operators
                return $"({this.Name}{this.Operator.ToValue()}{quote}{this.Value}{quote})";
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
