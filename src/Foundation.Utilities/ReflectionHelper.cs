namespace Naos.Foundation
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public static class ReflectionHelper
    {
        public static void SetProperties(object instance, IDictionary<string, object> propertyItems)
        {
            if (instance == null || propertyItems.IsNullOrEmpty())
            {
                return;
            }

            // or use https://github.com/ekonbenefits/dynamitey/wiki/UsageReallyLateBinding dynamic InvokeSetAll(object target, ...) =CASESENSITIVE
            foreach (var propertyInfo in instance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                foreach (var propertyItem in propertyItems.Safe())
                {
                    var propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;

                    if (propertyItem.Key.SafeEquals(propertyInfo.Name) && propertyItem.Value != null && propertyInfo.CanWrite)
                    {
                        propertyInfo.SetValue(instance, propertyItem.Value.To(propertyType), null);
                    }
                }
            }
        }
    }
}
