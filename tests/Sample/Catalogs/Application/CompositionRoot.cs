namespace Naos.Sample.Catalogs.Application
{
    using EnsureThat;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;
    using Naos.Foundation.Domain;
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
                    //.ConnectionString("Server=127.0.0.1;Database=naos_sample;User=sa;Password=Abcd1234!;Trusted_Connection=False;MultipleActiveResultSets=True;") // docker
                    //.ConnectionString(configuration.ConnectionString ?? "Server=(localdb)\\mssqllocaldb;Database=naos_sample;Trusted_Connection=True;MultipleActiveResultSets=True;")
                    .ConnectionString(configuration.ConnectionString)
                    .Schema(configuration.SchemaName.EmptyToNull() ?? "catalogs")
                    .AddIndex(p => p.Name)
                    .AddIndex(p => p.Region)
                    .AddIndex(p => p.Price)
                    .AddIndex(p => p.HasStock)));

            options.Context.Services.AddScoped<IProductRepository>(sp =>
            {
                return new ProductRepository(
                            //new RepositoryTracingDecorator<Product>(
                            //    sp.GetService<ILogger<ProductRepository>>(),
                            //    sp.GetService<ITracer>(),
                            //    new RepositoryLoggingDecorator<Product>(
                            //        sp.GetRequiredService<ILogger<ProductRepository>>(),
#pragma warning disable SA1114 // Parameter list should follow declaration
                            new DocumentRepository<Product>(o => o
                                .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                                .Provider(sp.GetRequiredService<IDocumentProvider<Product>>())
                                .Mediator(sp.GetRequiredService<IMediator>())));
#pragma warning restore SA1114 // Parameter list should follow declaration
            });

            //options.Context.Services.AddSingleton<IDocumentProvider<Product>>(sp =>
            //    new SqliteDocumentProvider<Product>(new SqliteDocumentProviderOptionsBuilder<Product>()
            //        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
            //        .EnableSqlLogging()
            //        .ConnectionString(@"Data Source=c:\tmp\naos_sample_catalogs.db;Version=3;")
            //        .AddIndex(p => p.Name)
            //        .AddIndex(p => p.Region)
            //        .AddIndex(p => p.Price)
            //        .AddIndex(p => p.HasStock).Build()));

            options.Context.Messages.Add("naos services builder: catalogs service added");

            options.Context.Services.AddHealthChecks()
                .AddSqlServer(configuration.ConnectionString, name: "Catalogs-sqlserver");

            return options;
        }
    }
}