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
    using Naos.Core.FileStorage.Domain;
    using Naos.Core.FileStorage.Infrastructure.FileSystem;
    using NSubstitute;
    using Xunit;

    public class FileStorageTestsBase
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

                Assert.Equal(3, (await storage.GetFileInformationsAsync()).Count());
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
                await storage.SaveFileContentsAsync("test.txt", "test");
                var file = (await storage.GetFileInformationsAsync()).Single();
                Assert.NotNull(file);
                Assert.Equal("test.txt", file.Path);
                string content = await storage.GetFileContentsAsync("test.txt");
                Assert.Equal("test", content);
                await storage.RenameFileAsync("test.txt", "new.txt");
                Assert.Contains(await storage.GetFileInformationsAsync(), f => f.Path == "new.txt");
                await storage.DeleteFileAsync("new.txt");
                Assert.Empty(await storage.GetFileInformationsAsync());
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
                Assert.True(await storage.SaveFileContentsAsync("test.txt", "test"));
                Assert.True(await storage.RenameFileAsync("test.txt", @"archive\new.txt"));
                Assert.Equal("test", await storage.GetFileContentsAsync(@"archive\new.txt"));
                Assert.Single(await storage.GetFileInformationsAsync());

                Assert.True(await storage.SaveFileContentsAsync("test2.txt", "test2"));
                Assert.True(await storage.RenameFileAsync("test2.txt", @"archive\new.txt"));
                Assert.Equal("test2", await storage.GetFileContentsAsync(@"archive\new.txt"));
                Assert.Single(await storage.GetFileInformationsAsync());
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

            string readmeFile = this.GetTestFilePath();
            using (storage)
            {
                Assert.False(await storage.ExistsAsync("readme.txt"));

                using (var stream = /*new NonSeekableStream(*/File.Open(readmeFile, FileMode.Open, FileAccess.Read))/*)*/
                {
                    bool result = await storage.SaveFileAsync("readme.txt", stream);
                    Assert.True(result);
                }

                Assert.Single(await storage.GetFileInformationsAsync());
                Assert.True(await storage.ExistsAsync("readme.txt"));

                using (var stream = await storage.GetFileStreamAsync("readme.txt"))
                {
                    string result = await new StreamReader(stream).ReadToEndAsync();
                    Assert.Equal(File.ReadAllText(readmeFile), result);
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
                Assert.Equal(2, (await storage.GetFileInformationsAsync()).Count());

                await storage.DeleteFilesAsync(@"x");
                Assert.Empty(await storage.GetFileInformationsAsync());
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
                Assert.Equal(2, (await storage.GetFileInformationsAsync()).Count());
                Assert.Single(await storage.GetFileInformationsAsync(limit: 1));
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\*")).Count());
                Assert.Single(await storage.GetFileInformationsAsync(@"x\nested\*"));

                await storage.DeleteFilesAsync(@"x\*");

                Assert.Empty(await storage.GetFileInformationsAsync());
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
                Assert.Equal(3, (await storage.GetFileInformationsAsync()).Count());
                Assert.Single(await storage.GetFileInformationsAsync(limit: 1));
                Assert.Equal(3, (await storage.GetFileInformationsAsync(@"x\*")).Count());
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\nested\*")).Count());
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\*.txt")).Count());

                await storage.DeleteFilesAsync(@"x\*.txt");

                Assert.Single(await storage.GetFileInformationsAsync());
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
                Assert.Equal(3, (await storage.GetFileInformationsAsync()).Count());
                Assert.Single(await storage.GetFileInformationsAsync(limit: 1));
                Assert.Equal(3, (await storage.GetFileInformationsAsync(@"x\*")).Count());
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\nested\*")).Count());
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\*.txt")).Count());

                await storage.DeleteFilesAsync(@"x\nested");

                Assert.Single(await storage.GetFileInformationsAsync());
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
                Assert.Equal(5, (await storage.GetFileInformationsAsync()).Count());
                Assert.Single(await storage.GetFileInformationsAsync(limit: 1));
                Assert.Equal(5, (await storage.GetFileInformationsAsync(@"x\*")).Count());
                Assert.Equal(3, (await storage.GetFileInformationsAsync(@"x\nested\*")).Count());
                Assert.Equal(3, (await storage.GetFileInformationsAsync(@"x\*.txt")).Count());

                await storage.DeleteFilesAsync(@"x\nested\*.txt");

                Assert.Equal(3, (await storage.GetFileInformationsAsync()).Count());
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
                var files = (await storage.GetFileInformationsAsync()).ToList();
                if (files.Count > 0)
                {
                    await storage.DeleteFilesAsync(files);
                }

                Assert.Empty(await storage.GetFileInformationsAsync());
            }
        }

        protected virtual string GetTestFilePath()
        {
            var currentDirectory = new DirectoryInfo(PathHelper.ExpandPath(@"|DataDirectory|\"));
            var currentFilePath = Path.Combine(currentDirectory.FullName, "README.md");
            while (!File.Exists(currentFilePath) && currentDirectory.Parent != null)
            {
                currentDirectory = currentDirectory.Parent;
                currentFilePath = Path.Combine(currentDirectory.FullName, "README.md");
            }

            if (File.Exists(currentFilePath))
            {
                return currentFilePath;
            }

            throw new ApplicationException("Unable to find test README.md file in path hierarchy.");
        }

        protected virtual IFileStorage GetStorage()
        {
            return null;
        }
    }

#pragma warning disable SA1402 // File may only contain a single class
    public class PostInfo
#pragma warning restore SA1402 // File may only contain a single class
    {
        public int ApiVersion { get; set; }

        public string CharSet { get; set; }

        public string ContentEncoding { get; set; }

        public byte[] Data { get; set; }

        public string IpAddress { get; set; }

        public string MediaType { get; set; }

        public string ProjectId { get; set; }

        public string UserAgent { get; set; }
    }

#pragma warning disable SA1402 // File may only contain a single class
#pragma warning disable SA1204 // Static elements must appear before instance elements
    public static class StorageExtensions
#pragma warning restore SA1204 // Static elements must appear before instance elements
#pragma warning restore SA1402 // File may only contain a single class
    {
        public static async Task<PostInfo> GetEventPostAndSetActiveAsync(this IFileStorage storage, string path, ILogger logger = null)
        {
            PostInfo eventPostInfo = null;
            try
            {
                eventPostInfo = await storage.GetFileObjectAsync<PostInfo>(path);
                if (eventPostInfo == null)
                {
                    return null;
                }

                if (!await storage.ExistsAsync(path + ".x") && !await storage.SaveFileContentsAsync(path + ".x", string.Empty))
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (logger != null && logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(ex, "Error retrieving event post data {Path}: {Message}", path, ex.Message);
                }

                return null;
            }

            return eventPostInfo;
        }

        public static async Task<bool> SetNotActiveAsync(this IFileStorage storage, string path, ILogger logger = null)
        {
            try
            {
                return await storage.DeleteFileAsync(path + ".x");
            }
            catch (Exception ex)
            {
                if (logger != null && logger.IsEnabled(LogLevel.Error))
                {
                    logger.LogError(ex, "Error deleting work marker {Path}: {Message}", path, ex.Message);
                }
            }

            return false;
        }

        public static async Task<bool> CompleteEventPostAsync(this IFileStorage storage, string path, string projectId, DateTime created, bool shouldArchive = true, ILogger logger = null)
        {
            // don't move files that are already in the archive
            if (path.StartsWith("archive"))
            {
                return true;
            }

            string archivePath = $"archive\\{projectId}\\{created.ToString("yy\\\\MM\\\\dd")}\\{Path.GetFileName(path)}";

            try
            {
                if (shouldArchive)
                {
                    if (!await storage.RenameFileAsync(path, archivePath))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!await storage.DeleteFileAsync(path))
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                if (logger != null && logger.IsEnabled(LogLevel.Error))
                {
                    logger?.LogError(ex, "Error archiving event post data {Path}: {Message}", path, ex.Message);
                }

                return false;
            }

            await storage.SetNotActiveAsync(path);

            return true;
        }
    }
}