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
            var connectionString = "DefaultEndpointsProtocol=https;AccountName=naos;AccountKey=BPBCZDqtZh8VsEmp4lKxC7PTODiZvb6D482NfdPXxbPK1BDwG1Cu66yid1MCgfZ5O1YpnPHUfpx1HiOs8D5hxw==;EndpointSuffix=core.windows.net";
            //var connectionString = "DefaultEndpointsProtocol=https;AccountName=naos-kv;AccountKey=0pmkGB6EMjHpNdQOBpsXthKEm3aolGaxE8lEMjs5xaphkMEVyRJCOKaE3WA98zBT4ZOeZQv4sUxbdKecQy7Gww==;TableEndpoint=https://naos-kv.table.cosmos.azure.com:443/;";

            if(!connectionString.IsNullOrEmpty())
            {
                return new TableKeyValueStorage(o => o
                    .ConnectionString(connectionString));
            }

            return null;
        }
    }
}
