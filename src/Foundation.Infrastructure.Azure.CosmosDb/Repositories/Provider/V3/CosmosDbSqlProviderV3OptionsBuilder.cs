namespace Naos.Foundation.Infrastructure
{
    using System;
    using System.Linq.Expressions;
    using Microsoft.Azure.Cosmos;

    public class CosmosDbSqlProviderV3OptionsBuilder<T> :
        BaseOptionsBuilder<CosmosDbSqlProviderV3Options<T>, CosmosDbSqlProviderV3OptionsBuilder<T>>
    {
        public CosmosDbSqlProviderV3OptionsBuilder<T> Client(CosmosClient client)
        {
            this.Target.Client = client;
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder<T> ConnectionString(string connectionString)
        {
            this.Target.ConnectionString = connectionString;
            this.Target.Client = new CosmosClient(connectionString);
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder<T> Account(string endPoint, string key)
        {
            this.Target.AccountEndPoint = endPoint;
            this.Target.AccountKey = key;
            this.Target.Client = new CosmosClient(endPoint, key);
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder<T> Database(string database)
        {
            this.Target.Database = database;
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder<T> Container(string container)
        {
            this.Target.Container = container;
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder<T> PartitionKey(string partitionKey)
        {
            this.Target.PartitionKey = partitionKey;
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder<T> PartitionKey(Expression<Func<T, string>> partitionKeyExpression)
        {
            this.Target.PartitionKeyStringExpression = partitionKeyExpression.Compile();
            this.Target.PartitionKey = $"/{partitionKeyExpression.ToExpressionString().Replace(".", "/")}";
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder<T> PartitionKey(Expression<Func<T, bool>> partitionKeyExpression)
        {
            this.Target.PartitionKeyBoolExpression = partitionKeyExpression.Compile();
            this.Target.PartitionKey = $"/{partitionKeyExpression.ToExpressionString().Replace(".", "/")}";
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder<T> PartitionKey(Expression<Func<T, double>> partitionKeyExpression)
        {
            this.Target.PartitionKeyDoubleExpression = partitionKeyExpression.Compile();
            this.Target.PartitionKey = $"/{partitionKeyExpression.ToExpressionString().Replace(".", "/")}";
            return this;
        }

        public CosmosDbSqlProviderV3OptionsBuilder<T> ThroughPut(int throughPut)
        {
            if(throughPut < 400)
            {
                throughPut = 400;
            }

            this.Target.ThroughPut = throughPut;
            return this;
        }
    }
}