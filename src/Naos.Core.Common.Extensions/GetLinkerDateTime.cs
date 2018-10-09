namespace Naos.Core.Common
{
    using System;
    using System.IO;
    using System.Reflection;

    public static partial class Extensions
    {
        /// <summary>
        /// Gets the linker date time for the assembly.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="target">The target timezone.</param>
        /// <returns></returns>
        public static DateTime GetLinkerDateTime(this Assembly assembly, TimeZoneInfo target = null)
        {
            var filePath = assembly.Location;
            const int cPeHeaderOffset = 60;
            const int cLinkerTimestampOffset = 8;

            var buffer = new byte[2048];

            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                stream.Read(buffer, 0, 2048);
            }

            var offset = BitConverter.ToInt32(buffer, cPeHeaderOffset);
            var secondsSince1970 = BitConverter.ToInt32(buffer, offset + cLinkerTimestampOffset);
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            var linkTimeUtc = epoch.AddSeconds(secondsSince1970);
            var timezone = target ?? TimeZoneInfo.Local;
            return TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, timezone);
        }
    }
}
