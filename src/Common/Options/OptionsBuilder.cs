namespace Naos.Core.Common
{
    public delegate TBuilder Builder<TBuilder, TOptions>(TBuilder builder)
        where TBuilder : class, IOptionsBuilder<TOptions>, new();

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
}
