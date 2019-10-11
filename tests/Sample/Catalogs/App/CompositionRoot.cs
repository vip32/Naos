namespace Microsoft.Extensions.DependencyInjection
{
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation.Infrastructure;
    using Naos.Sample.Catalogs.Domain;

    public static partial class CompositionRoot
    {
        public static ModuleOptions AddCatalogsModule(
            this ModuleOptions options,
            string section = "naos:sample:catalogs")
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNull(options.Context, nameof(options.Context));

            options.Context.AddTag("catalogs");

            var configuration = options.Context.Configuration?.GetSection($"{section}:sqlDocuments").Get<SqlDocumentsConfiguration>() ?? new SqlDocumentsConfiguration();

            options.Context.Services.AddSingleton<IDocumentProvider<Product>>(sp =>
                new SqlServerDocumentProvider<Product>(o => o
                    .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                    .EnableSqlLogging()
                    //.ConnectionString("Server=.;Database=naos_sample;User=sa;Password=Abcd1234!;Trusted_Connection=False;MultipleActiveResultSets=True;") // docker
                    .ConnectionString(configuration.ConnectionString ?? "Server=(localdb)\\mssqllocaldb;Database=naos_sample;Trusted_Connection=True;MultipleActiveResultSets=True;")
                    .Schema(configuration.SchemaName ?? "catalogs")
                    .AddIndex(p => p.Name)
                    .AddIndex(p => p.Region)
                    .AddIndex(p => p.Price)
                    .AddIndex(p => p.HasStock)));

            //options.Context.Services.AddSingleton<IDocumentProvider<Product>>(sp =>
            //    new SqliteDocumentProvider<Product>(new SqliteDocumentProviderOptionsBuilder<Product>()
            //        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
            //        .EnableSqlLogging()
            //        .ConnectionString(@"Data Source=c:\tmp\naos_sample_catalogs.db;Version=3;")
            //        .AddIndex(p => p.Name)
            //        .AddIndex(p => p.Region)
            //        .AddIndex(p => p.Price)
            //        .AddIndex(p => p.HasStock).Build()));

            options.Context.Messages.Add($"{LogKeys.Startup} naos services builder: catalogs service added");

            return options;
        }
    }
}