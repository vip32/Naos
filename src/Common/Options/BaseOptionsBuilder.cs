namespace Naos.Core.Common
{
    using Microsoft.Extensions.Logging;

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
}
