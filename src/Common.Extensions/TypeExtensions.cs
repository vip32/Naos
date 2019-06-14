namespace Naos.Foundation
{
    using System;
    using System.Linq;

    public static class TypeExtensions
    {
        public static bool IsOfType(this object source, Type targetType)
        {
            if(source == null)
            {
                return false;
            }

            return source.GetType() == targetType;
        }

        public static bool IsNotOfType(this object source, Type targetType)
        {
            if(source == null)
            {
                return false;
            }

            return source.GetType() != targetType;
        }

        public static string PrettyName(this Type source, bool useAngleBrackets = true)
        {
            if(source.IsGenericType)
            {
                var genericOpen = useAngleBrackets ? "<" : "[";
                var genericClose = useAngleBrackets ? ">" : "]";
                var name = source.Name.Substring(0, source.Name.IndexOf('`'));
                var types = string.Join(",", source.GetGenericArguments().Select(t => t.PrettyName()));
                return $"{name}{genericOpen}{types}{genericClose}";
            }

            return source.Name;
        }

        public static string FullPrettyName(this Type source, bool useAngleBrackets = true)
        {
            if(source.IsGenericType)
            {
                var genericOpen = useAngleBrackets ? "<" : "[";
                var genericClose = useAngleBrackets ? ">" : "]";
                var name = source.FullName.Substring(0, source.Name.IndexOf('`'));
                var types = string.Join(",", source.GetGenericArguments().Select(t => t.FullPrettyName()));
                return $"{name}{genericOpen}{types}{genericClose}";
            }

            return source.FullName;
        }

        public static bool IsNumeric(this Type type)
        {
            if(type.IsArray)
            {
                return false;
            }

            if(type == typeof(byte) ||
                type == typeof(decimal) ||
                type == typeof(double) ||
                type == typeof(short) ||
                type == typeof(int) ||
                type == typeof(long) ||
                type == typeof(sbyte) ||
                type == typeof(float) ||
                type == typeof(ushort) ||
                type == typeof(uint) ||
                type == typeof(ulong))
            {
                return true;
            }

            switch(Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
            }

            return false;
        }
    }
}
