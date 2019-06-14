namespace Naos.Foundation
{
    using System.Reflection;

    public static class MemberInfoExtensions
    {
        public static bool IsPropertyWithSetter(this MemberInfo source)
        {
            var property = source as PropertyInfo;
            return property?.SetMethod != null;
        }
    }
}
