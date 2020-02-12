namespace Naos.Foundation
{
    using Microsoft.Extensions.Logging;

    public class BaseOptionsBuilder<TOption, TBuilder> : OptionsBuilder<TOption>
        where TOption : OptionsBase, new()
        where TBuilder : BaseOptionsBuilder<TOption, TBuilder>
    {
        /// <summary>
        /// Sets the logger factory.
        /// </summary>
        /// <param name="loggerFactory">The logger factory.</param>
        public virtual TBuilder LoggerFactory(ILoggerFactory loggerFactory)
        {
            this.Target.LoggerFactory = loggerFactory;
            return (TBuilder)this;
        }
    }
}
