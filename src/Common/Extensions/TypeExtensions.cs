namespace Naos.Core.Common
{
    using System;
    using System.Linq;

    public static class TypeExtensions
    {
        public static bool IsOfType(this object source, Type targetType)
        {
            if (source == null)
            {
                return false;
            }

            return source.GetType() == targetType;
        }

        public static bool IsNotOfType(this object source, Type targetType)
        {
            if (source == null)
            {
                return false;
            }

            return source.GetType() != targetType;
        }

        public static string PrettyName(this Type source)
        {
            if (source.IsGenericType)
            {
                var name = source.Name.Substring(0, source.Name.IndexOf('`'));
                var types = string.Join(",", source.GetGenericArguments().Select(t => t.PrettyName()));
                return $"{name}<{types}>";
            }

            return source.Name;
        }
    }
}
