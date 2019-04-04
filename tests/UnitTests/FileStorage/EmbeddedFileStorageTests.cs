namespace Naos.Core.UnitTests.FileStorage
{
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Naos.Core.FileStorage.Domain;
    using Naos.Core.FileStorage.Infrastructure;
    using NSubstitute;
    using Xunit;

    public class EmbeddedFileStorageTests
    {
        [Fact]
        public async Task CanGetFileInfoAsync()
        {
            var storage = this.GetStorage();
            if(storage == null)
            {
                return;
            }

            using(storage)
            {
                var fileInfo = await storage.GetFileInformationAsync(@"Naos\Core\UnitTests/FileStorage\StubFile.txt");
                Assert.NotNull(fileInfo);
                Assert.True(fileInfo.ContentType == Core.Common.ContentType.TEXT);
            }
        }

        [Fact]
        public async Task CanGetJsonFileInfoAsync()
        {
            var storage = this.GetStorage();
            if(storage == null)
            {
                return;
            }

            using(storage)
            {
                var fileInfo = await storage.GetFileInformationAsync(@"Naos\Core\UnitTests/FileStorage\StubFile.json");
                Assert.NotNull(fileInfo);
                Assert.True(fileInfo.ContentType == Core.Common.ContentType.JSON);
            }
        }

        [Fact]
        public async Task CaHandleNonExistingFileInfoAsync()
        {
            var storage = this.GetStorage();
            if(storage == null)
            {
                return;
            }

            using(storage)
            {
                var fileInfo = await storage.GetFileInformationAsync(@"Naos\Core\UnitTests\FileStorage\DoesNotExist.txt");
                Assert.Null(fileInfo);
            }
        }

        [Fact]
        public async Task CanGetFileInfosAsync()
        {
            var storage = this.GetStorage();
            if(storage == null)
            {
                return;
            }

            using(storage)
            {
                var fileInfos = await storage.GetFileInformationsAsync();
                Assert.NotNull(fileInfos);
                Assert.NotNull(fileInfos.Files);
                Assert.True(fileInfos.Files.Any());
            }
        }

        [Fact]
        public async Task CanGetFileInfosByPatternAsync()
        {
            var storage = this.GetStorage();
            if(storage == null)
            {
                return;
            }

            using(storage)
            {
                var fileInfos = await storage.GetFileInformationsAsync(searchPattern: "*StubFile.*");
                Assert.NotNull(fileInfos);
                Assert.NotNull(fileInfos.Files);
                Assert.True(fileInfos.Files.Any());
            }
        }

        [Fact]
        public async Task CanGetFileStreamAsync()
        {
            var storage = this.GetStorage();
            if(storage == null)
            {
                return;
            }

            using(storage)
            {
                var stream = await storage.GetFileStreamAsync(@"Naos\Core\UnitTests/FileStorage\StubFile.txt");
                Assert.NotNull(stream);
                Assert.True(stream.Length > 0);
            }
        }

        [Fact]
        public async Task CanHandleNonExistingFileStreamAsync()
        {
            var storage = this.GetStorage();
            if(storage == null)
            {
                return;
            }

            using(storage)
            {
                var stream = await storage.GetFileStreamAsync(@"Naos\Core\UnitTests\FileStorage\DoesNotExist.txt");
                Assert.Null(stream);
            }
        }

        protected IFileStorage GetStorage()
        {
            return new EmbeddedFileStorage(o => o
                .LoggerFactory(Substitute.For<ILoggerFactory>())
                .Assembly(new[] { Assembly.GetExecutingAssembly() }));
        }
    }
}
