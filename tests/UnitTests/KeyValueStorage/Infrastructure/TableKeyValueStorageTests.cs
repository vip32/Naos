namespace Naos.Core.UnitTests.KeyValueStorage.Infrastructure
{
    using System.Threading.Tasks;
    using Naos.Core.Common;
    using Naos.Core.KeyValueStorage.Domain;
    using Naos.Core.KeyValueStorage.Infrastructure.Azure;
    using Xunit;

    public class TableKeyValueStorageTests : KeyValueStorageBaseTests
    {
        [Fact]
        public override Task InsertAndFindOne_ByKeys_Test()
        {
            return base.InsertAndFindOne_ByKeys_Test();
        }

        [Fact]
        public override Task InsertAndFindOne_ByKeys_Typed_Test()
        {
            return base.InsertAndFindOne_ByKeys_Typed_Test();
        }

        [Fact]
        public override Task InsertAndFindAll_ByCriteria_Typed_Test()
        {
            return base.InsertAndFindAll_ByCriteria_Typed_Test();
        }

        [Fact]
        public override Task CreateAndDeleteTable_Test()
        {
            return base.CreateAndDeleteTable_Test();
        }

        protected override IKeyValueStorage GetStorage()
        {
            //var connectionString = string.Empty;
            /*storage*/ // var connectionString = "DefaultEndpointsProtocol=https;AccountName=naos;AccountKey=mXME9AKmD4PV6ZJ2uCOw4/n3pDAHzw6kpQcjWtffB5PuMqLHTbbp2CuwJQd0vVQhTbA3/wVZPLgbMqwfG+uzrw==;EndpointSuffix=core.windows.net";
            /*cosmosdb*/ var connectionString = "DefaultEndpointsProtocol=https;AccountName=naos-kv;AccountKey=dWfn1piMz8TxVa9e4qezCZ2X497gSqBGQ2pZXZW61p1wjqeiacDhS9okl2hY7UttvYQe0fhJJ5eHvpUqJe4vHQ==;TableEndpoint=https://naos-kv.table.cosmos.azure.com:443/;";

            if(!connectionString.IsNullOrEmpty())
            {
                return new TableKeyValueStorage(o => o
                    .ConnectionString(connectionString));
            }

            return null;
        }
    }
}
