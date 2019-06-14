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
            const string BuildVersionMetadataPrefix1 = "+build";
            const string BuildVersionMetadataPrefix2 = ".build"; // TODO: make this an array of allowable prefixes

            var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if(attribute?.InformationalVersion != null)
            {
                var value = attribute.InformationalVersion;
                var prefix = BuildVersionMetadataPrefix1;
                var index = value.IndexOf(BuildVersionMetadataPrefix1);

                // fallback for '.build' prefix
                if(index == -1)
                {
                    prefix = BuildVersionMetadataPrefix2;
                    index = value.IndexOf(BuildVersionMetadataPrefix2);
                }

                if(index > 0)
                {
                    value = value.Substring(index + prefix.Length);
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
