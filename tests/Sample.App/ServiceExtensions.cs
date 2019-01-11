namespace Naos.Sample.App
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceExtensions
    {
        public static IServiceCollection AddNaosApp(
            this IServiceCollection services,
            IConfiguration configuration,
            string section = "naos:app:sample")
        {
            return services
                .AddOptions()
                .Configure<AppConfiguration>(configuration.GetSection(section));
        }
    }
}
