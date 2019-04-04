namespace Naos.Core.Common
{
    using System.Collections.Generic;

    public static partial class Extensions
    {
        //public static object TryGetValue(this Dictionary<string, object> source, string key, object @default = null)
        //{
        //    if (source.IsNullOrEmpty() || key.IsNullOrEmpty())
        //    {
        //        return @default;
        //    }

        //    if (source.TryGetValue(key, out object result))
        //    {
        //        return result;
        //    }

        //    return @default;
        //}

        public static T TryGetValue<T>(this Dictionary<string, T> source, string key, T @default = default)
        {
            if(source.IsNullOrEmpty() || key.IsNullOrEmpty())
            {
                return @default;
            }

            if(source.TryGetValue(key, out var result))
            {
                return result;
            }

            return @default;
        }
    }
}
