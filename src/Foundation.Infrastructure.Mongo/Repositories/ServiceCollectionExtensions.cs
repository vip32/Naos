namespace Microsoft.Extensions.DependencyInjection
{
    using System.Collections.Generic;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Bson.Serialization.Conventions;
    using MongoDB.Bson.Serialization.Serializers;
    using MongoDB.Driver;
    using MongoDB.Driver.Core.Events;
    using Naos.Foundation;
    using Naos.Foundation.Infrastructure;

    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMongoClient(
            this IServiceCollection services,
            MongoConfiguration configuration,
            IEnumerable<IConvention> conventions = null)
        {
            return services.AddMongoClient(null, configuration, conventions);
        }

        public static IServiceCollection AddMongoClient(
        this IServiceCollection services,
        string applicationName,
        MongoConfiguration configuration,
        IEnumerable<IConvention> conventions = null)
        {
            EnsureArg.IsNotNull(configuration, nameof(configuration));
            EnsureArg.IsNotNullOrEmpty(configuration.ConnectionString, nameof(configuration.ConnectionString));

            if (services == null)
            {
                return services;
            }

            services.AddSingleton<IMongoClient>(sp =>
            {
                if (BsonSerializer.LookupSerializer(typeof(decimal)) == null)
                {
                    BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));
                }

                if (BsonSerializer.LookupSerializer(typeof(decimal?)) == null)
                {
                    BsonSerializer.RegisterSerializer(typeof(decimal?),
                        new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));
                }

                // conventions are global
                if (conventions.SafeAny())
                {
                    var conventionPack = new ConventionPack();
                    conventionPack.AddRange(conventionPack);
                    // warning: because of filter these conventions will always be applied, for every type
                    ConventionRegistry.Register($"naos_conventions_{applicationName}", conventionPack, x => true);
                }
                else
                {
                    // warning: because of filter these conventions will always be applied, for every type
                    ConventionRegistry.Register($"naos_conventions_{applicationName}", new DefaultMongoConventions(), x => true);
                }

                var settings = MongoClientSettings.FromUrl(new MongoUrl(configuration.ConnectionString));
                settings.ApplicationName = applicationName; // for getservice<imongoclient> by app name
                if (configuration.LoggingEnabled)
                {
                    var logger = sp.GetRequiredService<ILogger>();
                    settings.ClusterConfigurator = c =>
                    {
                        c.Subscribe<CommandStartedEvent>(e =>
                        {
                            logger.LogDebug("{LogKey:l} execute mongo command: {@Command}", LogKeys.Infrastructure, e.Command.ToJson());
                        });
                    };
                }

                return new MongoClient(settings);
            });

            return services;
        }

        private sealed class DefaultMongoConventions : IConventionPack
        {
            public IEnumerable<IConvention> Conventions => new List<IConvention>
            {
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String),
                new CamelCaseElementNameConvention()
            };
        }

        private sealed class NoCasingMongoConventions : IConventionPack
        {
            public IEnumerable<IConvention> Conventions => new List<IConvention>
            {
                new IgnoreExtraElementsConvention(true),
                new EnumRepresentationConvention(BsonType.String)
            };
        }
    }
}
