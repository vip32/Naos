namespace Naos.Foundation
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;

    public static class EnumExtensions
    {
        public static string ToDescription(this Enum @enum)
        {
            var attribute = GetText<DescriptionAttribute>(@enum);

            return attribute.Description;
        }

        public static T GetText<T>(Enum @enum)
            where T : Attribute
        {
            var type = @enum.GetType();

            var memberInfo = type.GetMember(@enum.ToString());

            if(memberInfo != null && !memberInfo.Any())
            {
                throw new ArgumentException($"No public members for the argument '{@enum}'.");
            }

            var attributes = memberInfo[0].GetCustomAttributes(typeof(T), false);
            if(attributes == null || attributes.Length != 1)
            {
                throw new ArgumentException($"Can't find an attribute matching '{typeof(T).Name}' for the argument '{@enum}'");
            }

            return attributes.Single() as T;
        }

        /// <summary>
        /// Tries and parses an enum and it's default type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="value"></param>
        /// <returns>True if the enum value is defined.</returns>
        public static bool TryEnumIsDefined(Type type, object value)
        {
            if(type == null || value == null || !type.GetTypeInfo().IsEnum)
            {
                return false;
            }

            // Return true if the value is an enum and is a matching type.
            if(type == value.GetType())
            {
                return true;
            }

            if(TryEnumIsDefined<int>(type, value))
            {
                return true;
            }

            if(TryEnumIsDefined<string>(type, value))
            {
                return true;
            }

            if(TryEnumIsDefined<byte>(type, value))
            {
                return true;
            }

            if(TryEnumIsDefined<short>(type, value))
            {
                return true;
            }

            if(TryEnumIsDefined<long>(type, value))
            {
                return true;
            }

            if(TryEnumIsDefined<sbyte>(type, value))
            {
                return true;
            }

            if(TryEnumIsDefined<ushort>(type, value))
            {
                return true;
            }

            if(TryEnumIsDefined<uint>(type, value))
            {
                return true;
            }

            if(TryEnumIsDefined<ulong>(type, value))
            {
                return true;
            }

            return false;
        }

        public static bool TryEnumIsDefined<T>(Type type, object value)
        {
            // Catch any casting errors that can occur or if 0 is not defined as a default value.
            try
            {
                if(value is T && Enum.IsDefined(type, (T)value))
                {
                    return true;
                }
            }
#pragma warning disable RCS1075 // Avoid empty catch clause that catches System.Exception.
            catch(Exception)
#pragma warning restore RCS1075 // Avoid empty catch clause that catches System.Exception.
            {
                // return false;
            }

            return false;
        }
    }
}
