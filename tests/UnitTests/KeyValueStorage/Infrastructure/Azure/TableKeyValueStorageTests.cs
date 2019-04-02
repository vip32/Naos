namespace Naos.Core.UnitTests.KeyValueStorage.Infrastructure.Azure
{
    using System.Threading.Tasks;
    using Naos.Core.Common;
    using Naos.Core.KeyValueStorage.Domain;
    using Naos.Core.KeyValueStorage.Infrastructure.Azure;
    using Naos.Core.UnitTests.KeyValueStorage.Infrastructure;
    using Xunit;

    public class TableKeyValueStorageTests : KeyValueStorageBaseTests
    {
        [Fact]
        public override Task InsertAndGetOneValue_Test()
        {
            return base.InsertAndGetOneValue_Test();
        }

        [Fact]
        public override Task InsertAndGetOneTyped_Test()
        {
            return base.InsertAndGetOneTyped_Test();
        }

        [Fact]
        public override Task InsertAndGetAllWithCriteriaTyped_Test()
        {
            return base.InsertAndGetAllWithCriteriaTyped_Test();
        }

        [Fact]
        public override Task CreateAndDeleteTable_Test()
        {
            return base.CreateAndDeleteTable_Test();
        }

        protected override IKeyValueStorage GetStorage()
        {
            var connectionString = string.Empty;

            if (!connectionString.IsNullOrEmpty())
            {
                return new TableKeyValueStorage(o => o
                    .ConnectionString(connectionString));
            }

            return null;
        }
    }
}
