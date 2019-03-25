namespace Naos.Core.UnitTests.FileStorage
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;
    using Naos.Core.FileStorage.Infrastructure;
    using NSubstitute;
    using Xunit;

    public class FileStorageBaseTests
    {
        public virtual async Task CanGetEmptyFileListOnMissingDirectoryAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                Assert.Empty(await storage.GetFileInformationsAsync(Guid.NewGuid() + "\\*"));
            }
        }

        public virtual async Task CanGetFileListForSingleFolderAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                await storage.SaveFileContentsAsync(@"archived\archived.txt", "archived");
                await storage.SaveFileContentsAsync(@"q\new.txt", "new");
                await storage.SaveFileContentsAsync(@"long/path/in/here/1.hey.stuff-2.json", "archived");

                Assert.Equal(3, (await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage)).Count());
                Assert.Single(await storage.GetFileInformationsAsync(limit: 1));
                Assert.Single(await storage.GetFileInformationsAsync(@"long\path\in\here\*stuff*.json"));

                Assert.Single(await storage.GetFileInformationsAsync(@"archived\*"));
                Assert.Equal("archived", await storage.GetFileContentsAsync(@"archived\archived.txt"));

                Assert.Single(await storage.GetFileInformationsAsync(@"q\*"));
                Assert.Equal("new", await storage.GetFileContentsAsync(@"q\new.txt"));
            }
        }

        public virtual async Task CanGetFileInfoAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var fileInfo = await storage.GetFileInformationAsync(Guid.NewGuid().ToString());
                Assert.Null(fileInfo);

                var startTime = DateTime.UtcNow;
                Thread.Sleep(1000);

                string path = $"folder\\{Guid.NewGuid()}-nested.txt";
                Assert.True(await storage.SaveFileContentsAsync(path, "test"));
                fileInfo = await storage.GetFileInformationAsync(path);
                Assert.NotNull(fileInfo);
                Assert.True(fileInfo.Path.EndsWith("nested.txt"), "Incorrect file");
                Assert.True(fileInfo.Size > 0, "Incorrect file size");
                Assert.Equal(DateTimeKind.Utc, fileInfo.Created.Kind);
                // NOTE: File creation time might not be accurate: http://stackoverflow.com/questions/2109152/unbelievable-strange-file-creation-time-problem
                Assert.True(fileInfo.Created > DateTime.MinValue, "File creation time should be newer than the start time.");
                Assert.Equal(DateTimeKind.Utc, fileInfo.Modified.Kind);
                Assert.True(startTime <= fileInfo.Modified, $"File {path} modified time {fileInfo.Modified:O} should be newer than the start time {startTime:O}.");

                path = $"{Guid.NewGuid()}-test.txt";
                Assert.True(await storage.SaveFileContentsAsync(path, "test"));
                fileInfo = await storage.GetFileInformationAsync(path);
                Assert.NotNull(fileInfo);
                Assert.True(fileInfo.Path.EndsWith("test.txt"), "Incorrect file");
                Assert.True(fileInfo.Size > 0, "Incorrect file size");
                Assert.Equal(DateTimeKind.Utc, fileInfo.Created.Kind);
                Assert.True(fileInfo.Created > DateTime.MinValue, "File creation time should be newer than the start time.");
                Assert.Equal(DateTimeKind.Utc, fileInfo.Modified.Kind);
                Assert.True(startTime <= fileInfo.Modified, $"File {path} modified time {fileInfo.Modified:O} should be newer than the start time {startTime:O}.");
            }
        }

        public virtual async Task CanGetNonExistentFileInfoAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                await Assert.ThrowsAnyAsync<ArgumentException>(() => storage.GetFileInformationAsync(null));
                Assert.Null(await storage.GetFileInformationAsync(Guid.NewGuid().ToString()));
            }
        }

        public virtual async Task CanManageFilesAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var path1 = $"test-{Guid.NewGuid().ToString("N").Substring(10)}.txt";
                var path2 = $"test-{Guid.NewGuid().ToString("N").Substring(10)}.txt";

                await storage.SaveFileContentsAsync(path1, "test");
                var file = (await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage)).Single();
                Assert.NotNull(file);
                Assert.Equal(path1, file.Path);
                string content = await storage.GetFileContentsAsync(path1);
                Assert.Equal("test", content);
                await storage.RenameFileAsync(path1, path2);
                Assert.Contains(await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage), f => f.Path == path2);
                await storage.DeleteFileAsync(path2);
                Assert.Empty(await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage));
            }
        }

        public virtual async Task CanRenameFilesAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var path1 = $"test-{Guid.NewGuid().ToString("N").Substring(10)}.txt";
                var path2 = $"test-{Guid.NewGuid().ToString("N").Substring(10)}.txt";

                Assert.True(await storage.SaveFileContentsAsync(path1, "test"));
                Assert.True(await storage.RenameFileAsync(path1, @"archive\new.txt"));
                Assert.Equal("test", await storage.GetFileContentsAsync(@"archive\new.txt"));
                Assert.Single(await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage));

                Assert.True(await storage.SaveFileContentsAsync(path2, "test2"));
                Assert.True(await storage.RenameFileAsync(path2, @"archive\new.txt"));
                Assert.Equal("test2", await storage.GetFileContentsAsync(@"archive\new.txt"));
                Assert.Single(await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage));
            }
        }

        public virtual async Task CanSaveFilesAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var path = $"test-{Guid.NewGuid().ToString("N").Substring(10)}.txt";
                Assert.False(await storage.ExistsAsync(path));

                using (var stream = SerializationHelper.ToStream("test data"))
                {
                    bool result = await storage.SaveFileAsync(path, stream); // write
                    Assert.True(result);
                }

                Assert.Single(await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage));
                Assert.True(await storage.ExistsAsync(path));

                using (var stream = await storage.GetFileStreamAsync(path)) // read
                {
                    string result = await new StreamReader(stream).ReadToEndAsync();
                    Assert.Equal("test data", result);
                }
            }
        }

        public virtual async Task CanDeleteEntireFolderAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                await storage.SaveFileContentsAsync(@"x\hello.txt", "hello");
                await storage.SaveFileContentsAsync(@"x\nested\world.csv", "nested world");
                Assert.Equal(2, (await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage)).Count());

                await storage.DeleteFilesAsync(@"x");
                Assert.Empty(await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage));
            }
        }

        public virtual async Task CanDeleteEntireFolderWithWildcardAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                await storage.SaveFileContentsAsync(@"x\hello.txt", "hello");
                await storage.SaveFileContentsAsync(@"x\nested\world.csv", "nested world");
                Assert.Equal(2, (await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage)).Count());
                Assert.Single(await storage.GetFileInformationsAsync(limit: 1));
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\*")).Count());
                Assert.Single(await storage.GetFileInformationsAsync(@"x\nested\*"));

                await storage.DeleteFilesAsync(@"x\*");

                Assert.Empty(await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage));
            }
        }

        public virtual async Task CanDeleteSpecificFilesAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                await storage.SaveFileContentsAsync(@"x\hello.txt", "hello");
                await storage.SaveFileContentsAsync(@"x\nested\world.csv", "nested world");
                await storage.SaveFileContentsAsync(@"x\nested\hello.txt", "nested hello");
                Assert.Equal(3, (await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage)).Count());
                Assert.Single(await storage.GetFileInformationsAsync(limit: 1));
                Assert.Equal(3, (await storage.GetFileInformationsAsync(@"x\*")).Count());
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\nested\*")).Count());
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\*.txt")).Count());

                await storage.DeleteFilesAsync(@"x\*.txt");

                Assert.Single(await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage));
                Assert.False(await storage.ExistsAsync(@"x\hello.txt"));
                Assert.False(await storage.ExistsAsync(@"x\nested\hello.txt"));
                Assert.True(await storage.ExistsAsync(@"x\nested\world.csv"));
            }
        }

        public virtual async Task CanDeleteNestedFolderAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                await storage.SaveFileContentsAsync(@"x\hello.txt", "hello");
                await storage.SaveFileContentsAsync(@"x\nested\world.csv", "nested world");
                await storage.SaveFileContentsAsync(@"x\nested\hello.txt", "nested hello");
                Assert.Equal(3, (await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage)).Count());
                Assert.Single(await storage.GetFileInformationsAsync(limit: 1));
                Assert.Equal(3, (await storage.GetFileInformationsAsync(@"x\*")).Count());
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\nested\*")).Count());
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\*.txt")).Count());

                await storage.DeleteFilesAsync(@"x\nested");

                Assert.Single(await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage));
                Assert.True(await storage.ExistsAsync(@"x\hello.txt"));
                Assert.False(await storage.ExistsAsync(@"x\nested\hello.txt"));
                Assert.False(await storage.ExistsAsync(@"x\nested\world.csv"));
            }
        }

        public virtual async Task CanDeleteSpecificFilesInNestedFolderAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                await storage.SaveFileContentsAsync(@"x\hello.txt", "hello");
                await storage.SaveFileContentsAsync(@"x\world.csv", "world");
                await storage.SaveFileContentsAsync(@"x\nested\world.csv", "nested world");
                await storage.SaveFileContentsAsync(@"x\nested\hello.txt", "nested hello");
                await storage.SaveFileContentsAsync(@"x\nested\again.txt", "nested again");
                Assert.Equal(5, (await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage)).Count());
                Assert.Single(await storage.GetFileInformationsAsync(limit: 1));
                Assert.Equal(5, (await storage.GetFileInformationsAsync(@"x\*")).Count());
                Assert.Equal(3, (await storage.GetFileInformationsAsync(@"x\nested\*")).Count());
                Assert.Equal(3, (await storage.GetFileInformationsAsync(@"x\*.txt")).Count());

                await storage.DeleteFilesAsync(@"x\nested\*.txt");

                Assert.Equal(3, (await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage)).Count());
                Assert.True(await storage.ExistsAsync(@"x\hello.txt"));
                Assert.True(await storage.ExistsAsync(@"x\world.csv"));
                Assert.False(await storage.ExistsAsync(@"x\nested\hello.txt"));
                Assert.False(await storage.ExistsAsync(@"x\nested\again.txt"));
                Assert.True(await storage.ExistsAsync(@"x\nested\world.csv"));
            }
        }

        public virtual async Task CanRoundTripSeekableStreamAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                const string path = "user.xml";
                var element = XElement.Parse("<user>Blake</user>");

                using (var memoryStream = new MemoryStream())
                {
                    //logger.LogTrace("Saving xml to stream with position {Position}.", memoryStream.Position);
                    element.Save(memoryStream, SaveOptions.DisableFormatting);

                    memoryStream.Seek(0, SeekOrigin.Begin);
                    //logger.LogTrace("Saving contents with position {Position}", memoryStream.Position);
                    await storage.SaveFileAsync(path, memoryStream);
                    //logger.LogTrace("Saved contents with position {Position}.", memoryStream.Position);
                }

                using (var stream = await storage.GetFileStreamAsync(path))
                {
                    var actual = XElement.Load(stream);
                    Assert.Equal(element.ToString(SaveOptions.DisableFormatting), actual.ToString(SaveOptions.DisableFormatting));
                }
            }
        }

        public virtual async Task WillRespectStreamOffsetAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var path = "blake.txt";
                using (var memoryStream = new MemoryStream())
                {
                    long offset;
                    using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true))
                    {
                        writer.AutoFlush = true;
                        await writer.WriteAsync("Eric");
                        offset = memoryStream.Position;
                        await writer.WriteAsync("Blake");
                    }

                    memoryStream.Seek(offset, SeekOrigin.Begin);
                    await storage.SaveFileAsync(path, memoryStream);
                }

                Assert.Equal("Blake", await storage.GetFileContentsAsync(path));
            }
        }

        public virtual void CanUseDataDirectory()
        {
            const string DATA_DIRECTORY_QUEUE_FOLDER = @"|DataDirectory|\Queue";
            var storage = new FolderFileStorage(
                new FolderFileStorageOptions
                {
                    LoggerFactory = Substitute.For<ILoggerFactory>(),
                    Folder = DATA_DIRECTORY_QUEUE_FOLDER
                });
            Assert.NotNull(storage.Folder);
            Assert.NotEqual(DATA_DIRECTORY_QUEUE_FOLDER, storage.Folder);
            Assert.True(storage.Folder.EndsWith("Queue" + Path.DirectorySeparatorChar), storage.Folder);
        }

        protected async Task ResetAsync()
        {
            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var files = (await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage)).ToList();
                if (files.Count > 0)
                {
                    await storage.DeleteFilesAsync(files);
                }

                Assert.Empty(await Core.FileStorage.Domain.Extensions.GetFileInformationsAsync(storage));
            }
        }

        protected virtual IFileStorage GetStorage()
        {
            return null;
        }
    }
}