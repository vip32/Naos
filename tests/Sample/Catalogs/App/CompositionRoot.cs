namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;
    using Naos.Sample.Catalogs.Domain;

    public static partial class CompositionRoot
    {
        public static ModuleOptions AddCatalogsModule(
            this ModuleOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.AddTag("Catalogs");

            options.Context.Services.AddSingleton<IDocumentProvider<Product>>(sp =>
                new SqlServerDocumentProvider<Product>(o => o
                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .EnableSqlLogging()
                    .ConnectionString("Server=(localdb)\\mssqllocaldb;Database=naos_sample;Trusted_Connection=True;MultipleActiveResultSets=True;")
                    // Schema("catalogs")
                    .AddIndex(p => p.Name)
                    .AddIndex(p => p.Region)
                    .AddIndex(p => p.Price)
                    .AddIndex(p => p.HasStock)));

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: catalogs service added");

            return options;
        }
    }
}