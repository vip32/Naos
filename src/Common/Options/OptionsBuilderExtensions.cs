namespace Naos.Core.Common
{
    public static class OptionsBuilderExtensions
    {
        /// <summary>
        /// Provides acces to the target options for the specified builder.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder">The builder.</param>
        public static T Target<T>(this IOptionsBuilder builder)
        {
            return (T)builder.Target;
        }
    }
}
