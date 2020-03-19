namespace Naos.Sample.Application.Web
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    //using Microsoft.Extensions.Configuration;
    //using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Naos.Foundation;

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            await CreateHostBuilder(args).Build().RunAsync().AnyContext();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //.ConfigureAppConfiguration((ctx, cfg) => cfg.AddNaos(ctx))
                .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>())
                .UseDefaultServiceProvider((ctx, opt) => // https://andrewlock.net/new-in-asp-net-core-3-service-provider-validation/
                {
                    //options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                    opt.ValidateOnBuild = false; // TODO: turn validation on again
                });
    }
}
