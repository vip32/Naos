namespace Naos.Core.UnitTests.KeyValueStorage.Infrastructure
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Infrastructure;
    using Naos.Core.KeyValueStorage.Domain;
    using Naos.Core.KeyValueStorage.Infrastructure.FileStorage;
    using NSubstitute;
    using Xunit;

    public class FileStorageKeyValueStorageTests : KeyValueStorageBaseTests
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
            return new FileStorageKeyValueStorage(o => o
                .FileStorage(new FolderFileStorage(o => o
                    .LoggerFactory(Substitute.For<ILoggerFactory>())
                    .Serializer(new JsonNetSerializer())
                    .Folder(Path.Combine(Path.GetTempPath(), "naos_keyvaluestorage", "tests"))))); //o => o.Folder("|DataDirectory|\\temp"));
        }
    }
}
