namespace Naos.Core.Common
{
    using System.Reflection;

    public static class MemberInfoExtensions
    {
        internal static bool IsPropertyWithSetter(this MemberInfo member)
        {
            var property = member as PropertyInfo;
            return property?.SetMethod != null;
        }
    }
}
