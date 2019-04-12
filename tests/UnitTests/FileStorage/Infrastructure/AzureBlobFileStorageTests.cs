namespace Naos.Core.UnitTests.FileStorage.Infrastructure
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;
    using Naos.Core.FileStorage.Infrastructure;
    using NSubstitute;
    using Xunit;

    public class AzureBlobFileStorageTests : FileStorageBaseTests
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
            var connectionString = Configuration["naos:tests:storage:connectionString"];

            if(!connectionString.IsNullOrEmpty())
            {
                return new AzureBlobFileStorage(o => o
                    .LoggerFactory(Substitute.For<ILoggerFactory>())
                    .ConnectionString(connectionString));
            }

            return null;
        }
    }
}
