namespace Naos.Core.Common
{
    using System;
    using System.Linq;
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
            //if (value.IsNumber())
            //{
            //    if (value.TryTo(out int i))
            //    {
            //        this.Value = i;
            //    }
            //    else if (value.TryTo(out long l))
            //    {
            //        this.Value = l;
            //    }
            //    else if (value.TryTo(out decimal dc))
            //    {
            //        this.Value = dc;
            //    }
            //    else if (value.TryTo(out double d))
            //    {
            //        this.Value = d;
            //    }
            //}
            //else
            //{
            //    if (value.TryTo(out bool b))
            //    {
            //        this.Value = b;
            //    }
            //    else
            //    {
            this.Value = value ?? new object();
            //    }
            //}
        }

        public string Name { get; }

        public CriteriaOperator Operator { get; }

        public object Value { get; }

        public bool IsNumberValue() => this.Value.IsNumber();

        //public bool IsStringValue() => this.Value is string;

        //public bool IsIntValue() => this.Value is int;

        //public bool IsLongValue() => this.Value is long;

        //public bool IsDoubleValue() => this.Value is double;

        //public bool IsDecimalValue() => this.Value is decimal;

        //public bool IsBoolValue() => this.Value is bool;

        public Expression<Func<T, bool>> ToExpression<T>()
        {
            return ExpressionHelper.FromExpressionString<T>(this.ToString());
        }

        /// <summary>
        /// Returns a string representation for this specification
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var quote = this.IsNumberValue() ? string.Empty : "\"";
            if(this.Operator.IsFunction())
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
    }
}
