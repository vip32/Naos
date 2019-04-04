namespace Naos.Core.Common
{
    using System;
    using System.Globalization;
    using System.Reflection;

    public static class AssemblyExtensions
    {
        public static DateTime GetBuildDate(this Assembly assembly)
        {
            // origin: https://www.meziantou.net/2018/09/24/getting-the-date-of-build-of-a-net-assembly-at-runtime

            // note: project file needs to contain:
            //       <PropertyGroup><SourceRevisionId>build$([System.DateTime]::UtcNow.ToString("yyyyMMddHHmmss"))</SourceRevisionId></PropertyGroup>
            const string BuildVersionMetadataPrefix = "+build";

            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if(attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var index = value.IndexOf(BuildVersionMetadataPrefix);
                if(index > 0)
                {
                    value = value.Substring(index + BuildVersionMetadataPrefix.Length);
                    if(DateTime.TryParseExact(value, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                    {
                        return result;
                    }
                }
            }

            return default;
        }
    }
}
