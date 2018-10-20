namespace Naos.Sample.App.IntegrationTests
{
    using System;
    using Naos.Sample.App;

    public class BaseTest
    {
        public readonly AppConfiguration AppConfiguration = new AppConfiguration();

        public BaseTest()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Environment.SetEnvironmentVariable("ASPNETCORE_ISLOCAL", "True");

            this.AppConfiguration.Bind("naos:app:sample");
        }
    }
}