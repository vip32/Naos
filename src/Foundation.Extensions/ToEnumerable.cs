namespace Naos.Foundation
{
    using System.Collections.Generic;

    public static partial class Extensions
    {
        public static IEnumerable<T> ToEnumarable<T>(this T source)
        {
            yield return source;
        }
    }
}
