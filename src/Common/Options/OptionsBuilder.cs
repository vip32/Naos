namespace Naos.Core.Common
{
    public delegate TBuilder Builder<TBuilder, TOptions>(TBuilder builder)
        where TBuilder : class, IOptionsBuilder<TOptions>, new();

    public class OptionsBuilder<T> : IOptionsBuilder<T>
        where T : class, new()
    {
        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        public T Target { get; } = new T();

        /// <summary>
        /// Gets the target.
        /// </summary>
        /// <value>
        /// The target.
        /// </value>
        object IOptionsBuilder.Target => this.Target;

        /// <summary>
        /// Builds this options instance.
        /// </summary>
        /// <returns></returns>
        public virtual T Build()
        {
            return this.Target;
        }
    }
}
