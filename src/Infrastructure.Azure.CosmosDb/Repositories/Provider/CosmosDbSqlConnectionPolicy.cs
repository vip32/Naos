namespace Naos.Core.Infrastructure.Azure.CosmosDb
{
    public class CosmosDbSqlConnectionPolicy
    {
        public CosmosDbSqlConnectionMode ConnectionMode { get; set; }

        public CosmosDbSqlProtocol ConnectionProtocol { get; set; }
    }
}
