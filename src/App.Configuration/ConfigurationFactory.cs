//namespace Naos.Core.App
//{
//    using System;
//    using System.IO;
//    using Microsoft.Extensions.Configuration;
//    using Microsoft.Extensions.FileProviders;
//    using Microsoft.Extensions.Hosting;
//    using Microsoft.Extensions.Hosting.Internal;

//    public static class ConfigurationFactory
//    {
//        /// <summary>
//        /// Use for ASP.NET Core Web applications.
//        /// </summary>
//        /// <param name="config"></param>
//        /// <param name="env"></param>
//        /// <returns></returns>
//        public static IConfigurationBuilder Configure(IConfigurationBuilder config, IHostingEnvironment env)
//        {
//            return CreateConfigurationBuilder(config, env.EnvironmentName);
//        }

//        public static IConfigurationBuilder Configure(IConfigurationBuilder config, string environmentName = null)
//        {
//            return CreateConfigurationBuilder(config, environmentName);
//        }

//        /// <summary>
//        /// Use for .NET Core Console applications.
//        /// </summary>
//        /// <returns></returns>
//        //public static IConfigurationBuilder Configure(IConfigurationBuilder config, IHostingEnvironment env)
//        //{
//        //    return ConfigureInternal(config, env.EnvironmentName);
//        //}

//        /// <summary>
//        /// Use for .NET Core Console applications.
//        /// </summary>
//        /// <returns></returns>
//        public static IConfigurationBuilder CreateConfigurationBuilder()
//        {
//            return Configure(
//                new ConfigurationBuilder(),
//                new HostingEnvironment
//                {
//                    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
//                    ApplicationName = AppDomain.CurrentDomain.FriendlyName,
//                    ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
//                    ContentRootFileProvider = new PhysicalFileProvider(AppDomain.CurrentDomain.BaseDirectory)
//                });
//        }

//        private static IConfigurationBuilder CreateConfigurationBuilder(IConfigurationBuilder config, string environmentName)
//        {
//            return config
//                .SetBasePath(Directory.GetCurrentDirectory())
//                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
//                .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
//                .AddEnvironmentVariables();
//        }
//    }
//}
