namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Linq.Expressions;

    public class IndexMap
    {
        public string Name { get; set; }

        public Type Type { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public class IndexMap<T> : IndexMap, IIndexMap<T>
    {
        public IndexMap(Expression<Func<T, object>> expression = null)
        {
            if (expression != null)
            {
                this.Name = ExpressionHelper.GetPropertyName(expression);
                this.Type = ExpressionHelper.GetProperty(expression).PropertyType;
                this.Expression = expression.Compile();
            }
        }

        public Func<T, object> Expression { get; set; }
    }
}
