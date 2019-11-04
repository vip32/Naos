namespace Xunit
{
    using System.Runtime.CompilerServices;
    using EnsureThat;
    using global::Xunit.Abstractions;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Utilities.Xunit;

    /// <summary>
    /// The <see cref="TestOutputHelperExtensions" /> class provides extension methods for the
    /// <see cref="ITestOutputHelper" />.
    /// </summary>
    public static class TestOutputHelperExtensions
    {
        /// <summary>
        /// Builds a logger from the specified test output.
        /// </summary>
        /// <param name="output">The test output.</param>
        /// <param name="memberName">
        /// The member to create the logger for. This is automatically populated using <see cref="CallerMemberNameAttribute" />
        /// </param>
        /// <returns>The logger.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="output" /> is <c>null</c>.</exception>
        public static ILogger CreateLogger(
            this ITestOutputHelper output,
            [CallerMemberName] string memberName = null)
        {
            return CreateLogger(output, null, memberName);
        }

        /// <summary>
        /// Builds a logger from the specified test output.
        /// </summary>
        /// <param name="output">The test output.</param>
        /// <param name="settings">Optional logging settings.</param>
        /// <param name="memberName">
        /// The member to create the logger for. This is automatically populated using <see cref="CallerMemberNameAttribute" />
        /// </param>
        /// <returns>The logger.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="output" /> is <c>null</c>.</exception>
        public static ILogger CreateLogger(
            this ITestOutputHelper output,
            LoggingSettings settings,
            [CallerMemberName] string memberName = null)
        {
            EnsureArg.IsNotNull(output, nameof(output));

            using (var factory = TestHelper.CreateLoggerFactory(output, settings))
            {
                return factory.CreateLogger(memberName);
            }
        }

        /// <summary>
        /// Builds a logger from the specified test output for the specified type.
        /// </summary>
        /// <typeparam name="T">The type to create the logger for.</typeparam>
        /// <param name="output">The test output.</param>
        /// <returns>The logger.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="output" /> is <c>null</c>.</exception>
        public static ILogger<T> CreateLogger<T>(this ITestOutputHelper output)
        {
            return CreateLogger<T>(output, null);
        }

        /// <summary>
        /// Builds a logger from the specified test output for the specified type.
        /// </summary>
        /// <typeparam name="T">The type to create the logger for.</typeparam>
        /// <param name="output">The test output.</param>
        /// <param name="settings">Optional logging settings.</param>
        /// <returns>The logger.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="output" /> is <c>null</c>.</exception>
        public static ILogger<T> CreateLogger<T>(this ITestOutputHelper output, LoggingSettings settings)
        {
            EnsureArg.IsNotNull(output, nameof(output));

            using (var factory = TestHelper.CreateLoggerFactory(output, settings))
            {
                return factory.CreateLogger<T>();
            }
        }

        public static ILoggerFactory CreateLoggerFactory(this ITestOutputHelper output, LoggingSettings settings = null)
        {
            EnsureArg.IsNotNull(output, nameof(output));

            var result = new LoggerFactory();
#pragma warning disable CA2000 // Dispose objects before losing scope
            var provider = new TestLoggerProvider(output, settings);
#pragma warning restore CA2000 // Dispose objects before losing scope
            result.AddProvider(provider);
            return result;
        }
    }
}
