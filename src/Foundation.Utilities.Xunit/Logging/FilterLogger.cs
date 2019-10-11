namespace Naos.Foundation.Utilities.Xunit
{
    using System;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The <see cref="FilterLogger" />  class provides common filtering logic to ensure log records
    /// are only written for enabled loggers where there is a formatted message and/or exception to log.
    /// </summary>
    public abstract class FilterLogger : ILogger
    {
        private readonly string nullFormatted = "[null]";

        public abstract IDisposable BeginScope<TState>(TState state);

        public abstract bool IsEnabled(LogLevel logLevel);

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            EnsureArg.IsNotNull(formatter, nameof(formatter));

            if (!this.IsEnabled(logLevel))
            {
                return;
            }

            var formattedMessage = this.FormatMessage(state, exception, formatter);

            if (this.ShouldFilter(formattedMessage, exception))
            {
                return;
            }

            this.WriteLogEntry(logLevel, eventId, state, formattedMessage, exception, formatter);
        }

        /// <summary>
        /// Returns the formatted log message.
        /// </summary>
        /// <typeparam name="TState">The type of state data to log.</typeparam>
        /// <param name="state">The state data to log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="formatter">The formatter that creates the log message.</param>
        /// <returns>The log message.</returns>
        protected string FormatMessage<TState>(
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            Ensure.That(formatter, nameof(formatter)).IsNotNull();

#pragma warning disable CA1062 // Validate arguments of public methods
            var formattedMessage = formatter(state, exception);
#pragma warning restore CA1062 // Validate arguments of public methods

            // Clear the message if it looks like a null formatted message
            if (formattedMessage == this.nullFormatted)
            {
                return null;
            }

            return formattedMessage;
        }

        /// <summary>
        /// Determines whether the log message should be filtered and not written.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <param name="exception">The exception to log.</param>
        /// <returns><c>true</c> if the log should not be written; otherwise <c>false</c>.</returns>
        protected virtual bool ShouldFilter(string message, Exception exception)
        {
            if (exception != null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Writes the log entry with the specified values.
        /// </summary>
        /// <typeparam name="TState">The type of state data to log.</typeparam>
        /// <param name="logLevel">The log level.</param>
        /// <param name="eventId">The event id.</param>
        /// <param name="state">The state data to log.</param>
        /// <param name="message">The formatted message.</param>
        /// <param name="exception">The exception to log.</param>
        /// <param name="formatter">The formatter that creates the log message.</param>
        protected abstract void WriteLogEntry<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            string message,
            Exception exception,
            Func<TState, Exception, string> formatter);
    }
}