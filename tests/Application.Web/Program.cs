namespace Application.Web3
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
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
                .ConfigureAppConfiguration((ctx, cfg) => cfg.AddNaos(ctx))
                .ConfigureWebHostDefaults(builder =>
                {
                    builder
                        .UseStartup<Startup>()
                        .UseSerilog();
                })
                .UseDefaultServiceProvider((ctx, opt) => // https://andrewlock.net/new-in-asp-net-core-3-service-provider-validation/
                {
                    //options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                    opt.ValidateOnBuild = false; // TODO: turn validation on again
                });
    }
}
