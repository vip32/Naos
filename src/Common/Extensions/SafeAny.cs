namespace Naos.Core.Common
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public static partial class Extensions
    {
        [DebuggerStepThrough]
        public static bool SafeAny<T>(
            this IEnumerable<T> source,
            Func<T, bool> predicate = null)
        {
            if(source.IsNullOrEmpty())
            {
                return false;
            }

            if(predicate != null)
            {
                return source.Any(predicate);
            }

            return source.Any();
        }
    }
}
