namespace Application.Web3
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    using Naos.Configuration.App;
    using Naos.Foundation;
    using Serilog;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync().AnyContext();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                    NaosConfigurationFactory.Extend(config, args, context.HostingEnvironment.EnvironmentName))
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseSerilog();
                })
                .UseDefaultServiceProvider((context, options) => // https://andrewlock.net/new-in-asp-net-core-3-service-provider-validation/
                {
                    //options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                    options.ValidateOnBuild = false; // TODO: turn validation on again
                });
    }
}
