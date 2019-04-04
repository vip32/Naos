namespace Naos.Core.UnitTests.KeyValueStorage.Infrastructure.Azure
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
            //var connectionString = "DefaultEndpointsProtocol=https;AccountName=naos;AccountKey=iY7rvsvpzKxa3h8HED6B8VUB5V0NFur91zBr1F+Ebuttm9y0gjuFZScFdqbJeDBKydxiquXZpcSbA4/1iuZorg==;EndpointSuffix=core.windows.net";
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=naos-kv;AccountKey=KHmWrdnHt0J9myHiGZISAQJ8yBZOUNLi68JQDzZEipvvfyIDN2cCwGFPeeHcR9jXL8FQtwE7XqwCokF9tGDPpA==;TableEndpoint=https://naos-kv.table.cosmos.azure.com:443/;";

            if(!connectionString.IsNullOrEmpty())
            {
                return new TableKeyValueStorage(o => o
                    .ConnectionString(connectionString));
            }

            return null;
        }
    }
}
