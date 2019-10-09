namespace Microsoft.Extensions.DependencyInjection
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using EnsureThat;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using MongoDB.Driver;
    using Naos.Foundation.Domain;
    using Naos.Foundation.Infrastructure;
    using Naos.Operations.App;
    using Naos.Operations.Domain;
    using Naos.Operations.Infrastructure.Mongo;
    using Naos.Tracing.Domain;

    [ExcludeFromCodeCoverage]
    public static class NaosExtensions
    {
        public static INaosBuilderContext AddMongoLogging(this INaosBuilderContext context)
        {
            EnsureArg.IsNotNull(context, nameof(context));
            EnsureArg.IsNotNull(context.Services, nameof(context.Services));

            var configuration = context.Configuration?.GetSection("naos:operations:logging:mongo").Get<MongoLoggingConfiguration>();
            if (configuration != null)
            {
                context.Services.AddMongoClient("logging", new MongoConfiguration
                {
                    ConnectionString = configuration.ConnectionString,
                    DatabaseName = configuration.DatabaseName
                });

                context.Services.AddScoped<ILogEventRepository>(sp =>
                {
                    return new MongoLogEventRepository(o => o
                        .LoggerFactory(sp.GetRequiredService<ILoggerFactory>())
                        .MongoClient(sp.GetServices<MongoClient>()
                            .FirstOrDefault(c => c.Settings.ApplicationName == "logging"))
                        .Mapper(new AutoMapperEntityMapper(MapperFactory.Create()))
                        .CollectionName(configuration.CollectionName));
                });
                context.Messages.Add($"{LogKeys.Startup} naos services builder: logging azure mongo repository added (collection={configuration.CollectionName})");
            }

            return context;
        }
    }
}
