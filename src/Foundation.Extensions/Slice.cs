namespace Naos.Foundation
{
    using System;
    using System.Diagnostics;

    public static partial class Extensions
    {
        [DebuggerStepThrough]
        public static string Slice(this string source, string start, string end, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            return SliceFrom(source, start, comparison)
                .SliceTill(end, comparison);
        }

        [DebuggerStepThrough]
        public static string Slice(this string source, int start, int end)
        {
            if (source.IsNullOrEmpty())
            {
                return source;
            }

            if (end < 0)
            {
                end = source.Length + end;
            }

            return source.Substring(start, end - start);
            //return source[start..end];  // c#8 https://visualstudiomagazine.com/articles/2019/03/08/vs-2019-core-tip.aspx & https://csharp.christiannagel.com/2018/07/24/indexesandranges/77
        }
    }
}
