namespace Naos.Foundation.Infrastructure
{
    using System.Linq;

    public static partial class Extensions
    {
        public static IQueryable<T> TakeIf<T>(
            this IQueryable<T> source,
            int count = 100,
            bool condition = true)
        {
            if(condition && count > 0)
            {
                return source.Take(count);
            }

            return source;
        }
    }
}
