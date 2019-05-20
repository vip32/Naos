namespace Naos.Core.Common
{
    using System;
    using System.Diagnostics;

    public static partial class Extensions
    {
        [DebuggerStepThrough]
        public static string SliceTill(this string source, string till, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if(source.IsNullOrEmpty())
            {
                return source;
            }

            if(till.IsNullOrEmpty())
            {
                return source;
            }

            return SliceTillInternal(source, source.IndexOf(till, comparison));
        }

        [DebuggerStepThrough]
        public static string SliceTillLast(this string source, string till, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if(source.IsNullOrEmpty())
            {
                return source;
            }

            if(till.IsNullOrEmpty())
            {
                return source;
            }

            return SliceTillInternal(source, source.LastIndexOf(till, comparison));
        }

        private static string SliceTillInternal(this string source, int tillIndex)
        {
            if(source.IsNullOrEmpty())
            {
                return source;
            }

            if(tillIndex == 0)
            {
                return string.Empty;
            }

            if(tillIndex > 0)
            {
                return source.Substring(0, tillIndex);
            }

            return source;
        }
    }
}
