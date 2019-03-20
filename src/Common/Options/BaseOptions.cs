namespace Naos.Core.Common
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    public abstract class BaseOptions
    {
        /// <summary>
        /// Gets or sets the logger factory.
        /// </summary>
        /// <value>
        /// The logger factory.
        /// </value>
        public ILoggerFactory LoggerFactory { get; set; }

        public ILogger CreateLogger(string categoryName) => this.LoggerFactory == null ? NullLogger.Instance : this.LoggerFactory.CreateLogger(categoryName);

        public ILogger<T> CreateLogger<T>() => this.LoggerFactory == null ? new NullLogger<T>() : this.LoggerFactory.CreateLogger<T>();
    }
}
