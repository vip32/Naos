namespace Naos.Foundation
{
    using System;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    public abstract class OptionsBase
    {
        /// <summary>
        /// Gets or sets the logger factory.
        /// </summary>
        /// <value>
        /// The logger factory.
        /// </value>
        public ILoggerFactory LoggerFactory { get; set; }

        /// <summary>
        /// Creates the logger.
        /// </summary>
        /// <param name="categoryName">Name of the category.</param>
        public ILogger CreateLogger(string categoryName) =>
            this.LoggerFactory == null ? NullLogger.Instance : this.LoggerFactory.CreateLogger(categoryName);

        /// <summary>
        /// Creates the typed logger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public ILogger<T> CreateLogger<T>() =>
            this.LoggerFactory == null ? new NullLogger<T>() : this.LoggerFactory.CreateLogger<T>();

        /// <summary>
        /// Creates the typed logger.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public ILogger CreateLogger(Type type) =>
            this.LoggerFactory == null ? NullLogger.Instance : this.LoggerFactory.CreateLogger(type);
    }
}
