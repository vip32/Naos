namespace Naos.Sample.App.IntegrationTests
{
    using System;
    using Naos.Sample.App;

    public abstract class BaseTest
    {
        public static AppConfiguration AppConfiguration = new AppConfiguration();

        static BaseTest()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Environment.SetEnvironmentVariable("ASPNETCORE_ISLOCAL", "True");

            AppConfiguration.Bind("naos:app:sample");
        }
    }
}