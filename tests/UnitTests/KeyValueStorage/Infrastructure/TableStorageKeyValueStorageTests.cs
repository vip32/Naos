namespace Naos.Core.UnitTests.KeyValueStorage.Infrastructure
{
    using System.Threading.Tasks;
    using Naos.Core.Common;
    using Naos.Core.KeyValueStorage.Domain;
    using Naos.Core.KeyValueStorage.Infrastructure.Azure;
    using Xunit;

    public class TableStorageKeyValueStorageTests : KeyValueStorageBaseTests
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
            var connectionString = Configuration["naos:tests:storage:connectionString"];

            if(!connectionString.IsNullOrEmpty())
            {
                return new TableKeyValueStorage(o => o
                    .ConnectionString(connectionString));
            }

            return null;
        }
    }
}
