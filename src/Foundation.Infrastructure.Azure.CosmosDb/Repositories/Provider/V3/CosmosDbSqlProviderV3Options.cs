namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.Azure.Cosmos;

    public class CosmosDbSqlProviderV3Options<T> : BaseOptions
    {
        public CosmosClient Client { get; set; }

        public string ConnectionString { get; set; }

        public string AccountEndPoint { get; set; }

        public string AccountKey { get; set; }

        public string Database { get; set; } = "master";

        public string Container { get; set; }

        public string PartitionKey { get; set; }

        public int? ThroughPut { get; set; } = 400;

        public Func<T, string> PartitionKeyStringExpression { get; internal set; }

        public Func<T, bool> PartitionKeyBoolExpression { get; internal set; }

        public Func<T, double> PartitionKeyDoubleExpression { get; internal set; }
    }
}