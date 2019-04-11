namespace Naos.Core.UnitTests
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Naos.Core.Common;
    using Naos.Core.Configuration.App;

    public abstract class BaseTest
    {
        private static IConfigurationRoot configuration;

        static BaseTest()
        {
            Environment.SetEnvironmentVariable(EnvironmentKeys.Environment, "Development");
            Environment.SetEnvironmentVariable(EnvironmentKeys.IsLocal, "True");
        }

        public static IConfiguration Configuration
        {
            get
            {
                return configuration ?? (configuration = NaosConfigurationFactory.Create());
            }
        }
    }
}