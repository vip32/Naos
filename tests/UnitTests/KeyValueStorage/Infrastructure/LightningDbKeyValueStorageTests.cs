namespace Naos.UnitTests.KeyValueStorage.Infrastructure
{
    using System.IO;
    using System.Threading.Tasks;
    using Naos.KeyValueStorage.Domain;
    using Naos.KeyValueStorage.Infrastructure;
    using Xunit;

    public class LightningDbKeyValueStorageTests : KeyValueStorageBaseTests
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

        //[Fact]
        //public override Task InsertAndFindAll_ByCriteria_Typed_Test()
        //{
        //    return base.InsertAndFindAll_ByCriteria_Typed_Test();
        //}

        //[Fact]
        //public override Task CreateAndDeleteTable_Test()
        //{
        //    return base.CreateAndDeleteTable_Test();
        //}

        protected override IKeyValueStorage GetStorage()
        {
            return new LightningDbKeyValueStorage(o => o
                .Folder(Path.Combine(Path.GetTempPath(), "naos_keyvaluestorage", "tests", "lightning"))); //o => o.Folder("|DataDirectory|\\temp"));
        }
    }
}