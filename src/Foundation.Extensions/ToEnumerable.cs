namespace Naos.Foundation
{
    using System.Collections.Generic;
    using System.Linq;

    public static partial class Extensions
    {
        public static IEnumerable<T> ToEnumerable<T>(this T source)
        {
            yield return source;
        }

        //public static async Task<IEnumerable<T>> ToEnumerable<T>(this IAsyncEnumerable<T> source)
        //{
        //    if (source == null)
        //    {
        //        return null;
        //    }

        //    var result = new List<T>();
        //    await using (var enumerator = source.GetAsyncEnumerator())
        //    {
        //        while (await enumerator.MoveNextAsync())
        //        {
        //            result.Add(enumerator.Current);
        //        }
        //    }

        //    return result;
        //}

        public static IEnumerable<T> ToEnumerable<T>(this IAsyncEnumerable<T> source)
        {
            return AsyncEnumerable.ToEnumerable(source); // wraps System.Linq.Async::ToEnumerable from System.Interactive.Async
        }

        public static IAsyncEnumerable<T> ToAsyncEnumerable<T>(this IEnumerable<T> source)
        {
            return AsyncEnumerable.ToAsyncEnumerable(source); // wraps System.Linq.Async::ToAsyncEnumerable from System.Interactive.Async
        }
    }
}
