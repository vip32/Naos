namespace Naos.Foundation.Utilities.Xunit
{
    /// <summary>
    /// The <see cref="LoggingSettings" /> class is used to configure the <see cref="TestLogger"/>.
    /// </summary>
    public class LoggingSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoggingSettings" /> class.
        /// </summary>
        public LoggingSettings()
        {
            this.Formatter = new DefaultLogFormatter(this);
        }

        /// <summary>
        /// Gets or sets a custom formatting for rendering log messages to xUnit test output.
        /// </summary>
        public ILogFormatter Formatter { get; set; }

        /// <summary>
        /// Gets or sets whether exceptions thrown while logging outside of the test execution will be ignored.
        /// </summary>
        public bool IgnoreTestBoundaryException { get; set; }

        /// <summary>
        /// Identifies the number of spaces to use for indenting scopes.
        /// </summary>
        public int ScopePaddingSpaces { get; set; } = 3;
    }
}