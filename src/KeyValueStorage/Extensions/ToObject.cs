namespace Naos.KeyValueStorage
{
    using System;
    using System.Collections.Generic;
    using Naos.Foundation;

    public static partial class Extensions
    {
        public static IEnumerable<T> ToObject<T>(this IEnumerable<IDictionary<string, object>> sources)
            where T : class, new()
        {
            foreach (var source in sources.Safe())
            {
                yield return source.ToObject<T>();
            }
        }

        public static T ToObject<T>(this IDictionary<string, object> source)
            where T : class, new()
        {
            if (source == null)
            {
                return default;
            }

            var result = new T();
            var type = result.GetType();

            foreach (var item in source.Safe())
            {
                var prop = type.GetProperty(item.Key.Capitalize());
                if (prop != null)
                {
                    var propertyVal = Convert.ChangeType(item.Value, prop.PropertyType);

                    type.GetProperty(item.Key.Capitalize())
                        .SetValue(result, propertyVal, null);
                    // TODO: optionally don't break when something goes wrong
                    // ArgumentException, ArgumentNullException, InvalidCastException
                }
            }

            return result;
        }
    }
}
