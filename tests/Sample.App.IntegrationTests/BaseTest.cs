namespace Naos.Sample.App.IntegrationTests
{
    using System;
    using Naos.Sample.App;

#pragma warning disable RCS1102 // Make class static.
    public class BaseTest
#pragma warning restore RCS1102 // Make class static.
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