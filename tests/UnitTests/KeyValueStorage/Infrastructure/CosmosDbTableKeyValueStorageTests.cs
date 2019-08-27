namespace Naos.Core.UnitTests.KeyValueStorage.Infrastructure
{
    using System.Threading.Tasks;
    using Naos.Core.KeyValueStorage.Domain;
    using Naos.Core.KeyValueStorage.Infrastructure.Azure;
    using Naos.Foundation;
    using Xunit;

    public class CosmosDbTableKeyValueStorageTests : KeyValueStorageBaseTests
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
            var connectionString = string.Empty;
            //var connectionString = Configuration["naos:tests:cosmosDb:connectionString"];
            // WARN: Microsoft.Azure.Cosmos.Table has CAS issues with current nuget https://github.com/Azure/azure-cosmos-table-dotnet/issues/7 + https://forums.asp.net/t/2157664.aspx?System+MethodAccessException+at+CloudTable+CreateIfNotExists

            if (!connectionString.IsNullOrEmpty())
            {
                return new TableKeyValueStorage(o => o
                    .ConnectionString(connectionString));
            }

            return null;
        }
    }
}
