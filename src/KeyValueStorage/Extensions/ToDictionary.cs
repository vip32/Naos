namespace Naos.Core.KeyValueStorage
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static partial class Extensions
    {
        public static IDictionary<string, object> ToDictionary(
            this object source,
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            if (source == null)
            {
                return default;
            }

            return source.GetType().GetProperties(bindingAttr)
                .ToDictionary(
                    pi => pi.Name,
                    pi => pi.GetValue(source, null));
        }
    }
}
