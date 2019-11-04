namespace Xunit
{
    using System.Runtime.CompilerServices;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Utilities.Xunit;
    using Xunit.Abstractions;

    public static class TestHelper
    {
        public static ILogger CreateLogger(ITestOutputHelper output, LoggingSettings settings = null, [CallerMemberName] string memberName = null)
        {
            EnsureArg.IsNotNull(output, nameof(output));

            return output.CreateLogger(settings, memberName);
        }

        public static ILogger CreateLogger<T>(ITestOutputHelper output, LoggingSettings settings = null)
        {
            EnsureArg.IsNotNull(output, nameof(output));

            return output.CreateLogger<T>(settings);
        }

        public static ILoggerFactory CreateLoggerFactory(ITestOutputHelper output, LoggingSettings settings = null)
        {
            EnsureArg.IsNotNull(output, nameof(output));

            return output.CreateLoggerFactory(settings);
        }
    }
}
