namespace Naos.Core.Common
{
    using System;
    using System.ComponentModel;
    using System.Reflection;

    public static partial class Extensions
    {
        public static T ToType<T>(this object value)
        {
            var targetType = typeof(T);
            if (value == null)
            {
                try
                {
                    return (T)Convert.ChangeType(value, targetType);
                }
                catch
                {
                    throw new ArgumentNullException(nameof(value));
                }
            }

            var converter = TypeDescriptor.GetConverter(targetType);
            var valueType = value.GetType();

            if (targetType.IsAssignableFrom(valueType))
            {
                return (T)value;
            }

            var targetTypeInfo = targetType.GetTypeInfo();
            if (targetTypeInfo.IsEnum && (value is string || valueType.GetTypeInfo().IsEnum))
            {
                // attempt to match enum by name.
                if (EnumExtensions.TryEnumIsDefined(targetType, value.ToString()))
                {
                    object parsedValue = Enum.Parse(targetType, value.ToString(), false);
                    return (T)parsedValue;
                }

                throw new ArgumentException($"The Enum value of '{value}' is not defined as a valid value for '{targetType.FullName}'.");
            }

            if (targetTypeInfo.IsEnum && valueType.IsNumeric())
            {
                return (T)Enum.ToObject(targetType, value);
            }

            if (converter.CanConvertFrom(valueType))
            {
                object convertedValue = converter.ConvertFrom(value);
                return (T)convertedValue;
            }

            if (!(value is IConvertible))
            {
                throw new ArgumentException($"An incompatible value specified. Target Type: {targetType.FullName} Value Type: {value.GetType().FullName}", nameof(value));
            }

            try
            {
                object convertedValue = Convert.ChangeType(value, targetType);
                return (T)convertedValue;
            }
            catch (Exception e)
            {
                throw new ArgumentException($"An incompatible value specified. Target Type: {targetType.FullName} Value Type: {value.GetType().FullName}", nameof(value), e);
            }
        }
    }
}
