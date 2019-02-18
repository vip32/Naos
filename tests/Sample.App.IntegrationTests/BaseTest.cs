namespace Naos.Sample.App.IntegrationTests
{
    using System;
    using Naos.Core.Common;
    //using Naos.Sample.App;

    public abstract class BaseTest
    {
        //public static AppConfiguration AppConfiguration = new AppConfiguration();

        static BaseTest()
        {
            Environment.SetEnvironmentVariable(EnvironmentKeys.Environment, "Development");
            Environment.SetEnvironmentVariable(EnvironmentKeys.IsLocal, "True");

            //AppConfiguration.Bind("naos:app:sample");
        }
    }
}