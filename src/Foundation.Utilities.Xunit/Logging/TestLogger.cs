namespace Naos.Foundation.Utilities.Xunit
{
    using System;
    using System.Collections.Generic;
    using EnsureThat;
    using global::Xunit.Abstractions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The <see cref="TestLogger" /> class is used to provide a logging implementation for Xunit.
    /// </summary>
    public class TestLogger : FilterLoggerBase
    {
        private readonly LoggingSettings settings;
        private readonly ILogFormatter formatter;
        private readonly string name;
        private readonly ITestOutputHelper output;
        private readonly Stack<LoggingScopeWriter> scopes;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestLogger"/> class.
        /// Creates a new instance of the <see cref="TestLogger" /> class.
        /// </summary>
        /// <param name="name">The name of the logger.</param>
        /// <param name="output">The test output helper.</param>
        /// <param name="settings">Optional logging settings.</param>
        /// <exception cref="ArgumentException">The <paramref name="name" /> is <c>null</c>, empty or whitespace.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="output" /> is <c>null</c>.</exception>
        public TestLogger(string name, ITestOutputHelper output, LoggingSettings settings = null)
        {
            EnsureArg.IsNotNullOrWhiteSpace(name, nameof(name));
            EnsureArg.IsNotNull(output, nameof(output));

            this.name = name;
            this.output = output;
            this.settings = settings ?? new LoggingSettings();
            this.formatter = this.settings.Formatter ?? new DefaultLogFormatter(this.settings);
            this.scopes = new Stack<LoggingScopeWriter>();
        }

        public override IDisposable BeginScope<TState>(TState state)
        {
            var writer = new LoggingScopeWriter(this.output, state, this.scopes.Count, () => this.scopes.Pop(), this.settings);
            this.scopes.Push(writer);
            return writer;
        }

        public override bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        protected override void WriteLogEntry<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            string message,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            try
            {
                this.WriteLog(logLevel, eventId, message, exception);
            }
            catch (InvalidOperationException)
            {
                if (!this.settings.IgnoreTestBoundaryException)
                {
                    throw;
                }
            }
        }

        private void WriteLog(
            LogLevel logLevel,
            EventId eventId,
            string message,
            Exception exception)
        {
            var formattedMessage = this.formatter.Format(this.scopes.Count, this.name, logLevel, eventId, message, exception);

            this.output.WriteLine(formattedMessage);
        }
    }
}