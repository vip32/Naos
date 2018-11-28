namespace Naos.Sample.App.Web
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Naos.Core.App.Configuration;
    using Serilog;

    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    NaosConfigurationFactory.Extend(config);
                })
                .UseStartup<Startup>()
                .UseSerilog();
    }
}
