namespace Naos.Core.Common
{
    using Microsoft.Extensions.Logging;

    public static class OptionsBuilderExtensions
    {
        public static T Target<T>(this IOptionsBuilder builder)
        {
            return (T)builder.Target;
        }
    }
}
