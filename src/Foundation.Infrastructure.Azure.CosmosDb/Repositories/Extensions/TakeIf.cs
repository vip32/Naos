namespace Naos.Foundation.Infrastructure
{
    using System.Linq;

    public static partial class Extensions
    {
        public static IQueryable<T> TakeIf<T>(
            this IQueryable<T> source,
            int? count = null,
            bool? condition = true)
        {
            if(condition == true && count.HasValue && count.Value > 0)
            {
                return source.Take(count.Value);
            }

            return source;
        }
    }
}
