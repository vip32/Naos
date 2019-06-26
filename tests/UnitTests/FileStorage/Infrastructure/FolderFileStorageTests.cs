namespace Naos.Core.UnitTests.FileStorage.Infrastructure
{
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.FileStorage.Domain;
    using Naos.Core.FileStorage.Infrastructure;
    using Naos.Foundation;
    using NSubstitute;
    using Xunit;

    public class FolderFileStorageTests : FileStorageBaseTests
    {
        [Fact]
        public override Task CanGetEmptyFileListOnMissingDirectoryAsync()
        {
            return base.CanGetEmptyFileListOnMissingDirectoryAsync();
        }

        [Fact]
        public override Task CanGetFileListForSingleFolderAsync()
        {
            return base.CanGetFileListForSingleFolderAsync();
        }

        [Fact]
        public override Task CanGetFileInfoAsync()
        {
            return base.CanGetFileInfoAsync();
        }

        [Fact]
        public override Task CanGetNonExistentFileInfoAsync()
        {
            return base.CanGetNonExistentFileInfoAsync();
        }

        [Fact]
        public override Task CanSaveFilesAsync()
        {
            return base.CanSaveFilesAsync();
        }

        [Fact]
        public override Task CanSaveCsvFileAsync()
        {
            return base.CanSaveCsvFileAsync();
        }

        [Fact]
        public override Task CanSaveCsvWithCustomHeaderFileAsync()
        {
            return base.CanSaveCsvWithCustomHeaderFileAsync();
        }

        [Fact]
        public override Task CanSaveCsvDictionaryFileAsync()
        {
            return base.CanSaveCsvDictionaryFileAsync();
        }

        [Fact]
        public async Task CanSaveFilesWithSerializersAsync()
        {
            await base.CanSaveFilesWithSerializerAsync(new Base64Serializer(), "base64");
            await base.CanSaveFilesWithSerializerAsync(new BsonDataSerializer(), "bson");
            await base.CanSaveFilesWithSerializerAsync(new HexSerializer(), "hex");
            await base.CanSaveFilesWithSerializerAsync(new JsonNetSerializer(), "json");
            await base.CanSaveFilesWithSerializerAsync(new MessagePackSerializer(), "mpack");
            await base.CanSaveFilesWithSerializerAsync(new CsvSerializer(), "csv");
        }

        [Fact]
        public override Task CanManageFilesAsync()
        {
            return base.CanManageFilesAsync();
        }

        [Fact]
        public override Task CanRenameFilesAsync()
        {
            return base.CanRenameFilesAsync();
        }

        [Fact]
        public override void CanUseDataDirectory()
        {
            base.CanUseDataDirectory();
        }

        [Fact]
        public override Task CanDeleteEntireFolderAsync()
        {
            return base.CanDeleteEntireFolderAsync();
        }

        [Fact]
        public override Task CanDeleteEntireFolderWithWildcardAsync()
        {
            return base.CanDeleteEntireFolderWithWildcardAsync();
        }

        [Fact]
        public override Task CanDeleteSpecificFilesAsync()
        {
            return base.CanDeleteSpecificFilesAsync();
        }

        [Fact]
        public override Task CanDeleteNestedFolderAsync()
        {
            return base.CanDeleteNestedFolderAsync();
        }

        [Fact]
        public override Task CanDeleteSpecificFilesInNestedFolderAsync()
        {
            return base.CanDeleteSpecificFilesInNestedFolderAsync();
        }

        [Fact]
        public override Task CanRoundTripSeekableStreamAsync()
        {
            return base.CanRoundTripSeekableStreamAsync();
        }

        //[Fact]
        //public override Task WillRespectStreamOffsetAsync()
        //{
        //    return base.WillRespectStreamOffsetAsync();
        //}

        protected override IFileStorage GetStorage()
        {
            return new FolderFileStorage(o => o
                .LoggerFactory(Substitute.For<ILoggerFactory>())
                .Folder(Path.Combine(Path.GetTempPath(), "naos_filestorage", "tests_normal"))); //o => o.Folder("|DataDirectory|\\temp"));
        }
    }
}
