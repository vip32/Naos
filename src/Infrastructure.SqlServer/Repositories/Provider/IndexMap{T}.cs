namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class IndexMap<T> : IIndexMap<T>
    {
        public IndexMap(string name, Expression<Func<T, object>> value = null,
            Expression<Func<T, IEnumerable<object>>> values = null, string description = null)
        {
            this.Name = name;

            if (value != null)
            {
                this.Value = value.Compile();
            }

            if (values != null)
            {
                this.Values = values.Compile();
            }

            this.Description = description;
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public Func<T, object> Value { get; set; }

        public Func<T, IEnumerable<object>> Values { get; set; }
    }
}
