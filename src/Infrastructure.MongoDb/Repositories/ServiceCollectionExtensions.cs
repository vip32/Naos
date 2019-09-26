namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.Extensions.Logging;
    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.Core.Events;
    using Naos.Foundation;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoClient(this IServiceCollection services, string connectionString = null)
        {
            if (services == null)
            {
                return services;
            }

            services.AddSingleton<IMongoClient>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger>();
                var mongoClientSettings = MongoClientSettings.FromUrl(
                    new MongoUrl(connectionString.EmptyToNull() ?? "mongodb://localhost:27017?connectTimeoutMS=300000"));
                mongoClientSettings.ClusterConfigurator = c =>
                {
                    c.Subscribe<CommandStartedEvent>(e =>
                    {
                        logger.LogInformation($"{e.CommandName} - {e.Command.ToJson()}");
                    });
                };

                return new MongoClient(mongoClientSettings);
            });

            return services;
        }
    }
}
