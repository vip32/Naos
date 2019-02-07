namespace Naos.Core.Common
{
    using Microsoft.Extensions.Logging;

    public abstract class BaseOptions
    {
        public ILoggerFactory LoggerFactory { get; set; }
    }
}
