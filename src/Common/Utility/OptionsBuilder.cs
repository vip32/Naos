namespace Naos.Core.Common
{
#pragma warning disable SA1402 // File may only contain a single class
#pragma warning disable SA1201 // Elements must appear in the correct order
    public class OptionsBuilder<T> : IOptionsBuilder<T>
        where T : class, new()
    {
        public T Target { get; } = new T();

        object IOptionsBuilder.Target => this.Target;

        public virtual T Build()
        {
            return this.Target;
        }
    }

    public delegate TBuilder Builder<TBuilder, TOptions>(TBuilder builder)
        where TBuilder : class, IOptionsBuilder<TOptions>, new();

    public static class OptionsBuilderExtensions
    {
        public static T Target<T>(this IOptionsBuilder builder)
        {
            return (T)builder.Target;
        }
    }
#pragma warning restore SA1201 // Elements must appear in the correct order
#pragma warning restore SA1402 // File may only contain a single class
}
