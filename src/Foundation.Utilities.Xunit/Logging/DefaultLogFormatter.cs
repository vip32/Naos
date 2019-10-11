namespace Naos.Foundation.Utilities.Xunit
{
    using System;
    using System.Globalization;
    using System.Text;
    using EnsureThat;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The <see cref="DefaultLogFormatter" /> class provides the default formatting of log messages for xUnit test output.
    /// </summary>
    public class DefaultLogFormatter : ILogFormatter
    {
        private readonly LoggingSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultLogFormatter"/> class.
        /// </summary>
        /// <param name="settings">The logging settings.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="settings"/> value is <c>null</c>.</exception>
        public DefaultLogFormatter(LoggingSettings settings)
        {
            EnsureArg.IsNotNull(settings, nameof(settings));

            this.settings = settings;
        }

        public string Format(
            int scopeLevel,
            string name,
            LogLevel logLevel,
            EventId eventId,
            string message,
            Exception exception)
        {
            const string Format = "{0}{2} [{3}]: {4}";
            var padding = new string(' ', scopeLevel * this.settings.ScopePaddingSpaces);

            var builder = new StringBuilder();

            if (!string.IsNullOrWhiteSpace(message))
            {
                builder.AppendFormat(CultureInfo.InvariantCulture,
                    Format,
                    padding,
                    name,
                    logLevel,
                    eventId.Id,
                    message);
                builder.AppendLine();
            }

            if (exception != null)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture,
                    Format,
                    padding,
                    name,
                    logLevel,
                    eventId.Id,
                    exception);
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}