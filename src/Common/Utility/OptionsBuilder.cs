using Microsoft.Extensions.Logging;

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

    public abstract class BaseOptions
    {
        public ILoggerFactory LoggerFactory { get; set; }
    }

    public class BaseOptionsBuilder<TOption, TBuilder> : OptionsBuilder<TOption>
        where TOption : BaseOptions, new()
        where TBuilder : BaseOptionsBuilder<TOption, TBuilder>
    {
        public TBuilder LoggerFactory(ILoggerFactory loggerFactory)
        {
            this.Target.LoggerFactory = loggerFactory;
            return (TBuilder)this;
        }
    }
#pragma warning restore SA1201 // Elements must appear in the correct order
#pragma warning restore SA1402 // File may only contain a single class
}
