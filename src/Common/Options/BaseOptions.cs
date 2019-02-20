namespace Naos.Core.Common
{
    using Microsoft.Extensions.Logging;

    public abstract class BaseOptions
    {
        /// <summary>
        /// Gets or sets the logger factory.
        /// </summary>
        /// <value>
        /// The logger factory.
        /// </value>
        public ILoggerFactory LoggerFactory { get; set; } // use FileStorageLoggingDecorator
    }
}
