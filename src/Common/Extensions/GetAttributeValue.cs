namespace Naos.Core.Common
{
    using System;
    using System.Linq;
    using System.Reflection;

    public static partial class Extensions
    {
        public static TE GetAttributeValue<T, TE>(this Enum enumeration, Func<T, TE> expression)
        where T : Attribute
        {
            var attribute =
              enumeration
                .GetType()
                .GetMember(enumeration.ToString())
                .Where(member => member.MemberType == MemberTypes.Field)
                .FirstOrDefault()
                .GetCustomAttributes(typeof(T), false)
                .Cast<T>()
                .SingleOrDefault();

            if (attribute == null)
            {
                return default(TE);
            }

            return expression(attribute);
        }

        public static TValue GetAttributeValue<TAttribute, TValue>(
                this Type type,
                Func<TAttribute, TValue> valueSelector)
                where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            if (att != null)
            {
                return valueSelector(att);
            }

            return default(TValue);
        }
    }
}