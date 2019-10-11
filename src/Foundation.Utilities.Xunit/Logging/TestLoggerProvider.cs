namespace Naos.Foundation.Utilities.Xunit
{
    using EnsureThat;
    using global::Xunit.Abstractions;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The <see cref="TestLoggerProvider" /> class is used to provide Xunit logging for <see cref="ILoggerFactory" />
    /// </summary>
    public class TestLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper output;
        private readonly LoggingSettings settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestLoggerProvider" /> class.
        /// </summary>
        /// <param name="output">The test output.</param>
        /// <param name="settings">Optional logging settings.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="output" /> is <c>null</c>.</exception>
        public TestLoggerProvider(ITestOutputHelper output, LoggingSettings settings = null)
        {
            EnsureArg.IsNotNull(output, nameof(output));

            this.output = output;
            this.settings = settings;
        }

        public ILogger CreateLogger(string categoryName)
        {
            EnsureArg.IsNotNullOrEmpty(categoryName, nameof(categoryName));

            return new TestLogger(categoryName, this.output, this.settings);
        }

        public void Dispose()
        {
            // do nothing
        }
    }
}
