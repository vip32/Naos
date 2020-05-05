namespace Naos.UnitTests.FileStorage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using Microsoft.Extensions.Logging;
    using Naos.FileStorage.Csv.Domain;
    using Naos.FileStorage.Domain;
    using Naos.FileStorage.Infrastructure;
    using Naos.Foundation;
    using NSubstitute;
    using Xunit;

    public class FileStorageBaseTests : BaseTests
    {
#pragma warning disable CA2007 // Consider calling ConfigureAwait on the awaited task
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

                Assert.Equal(3, (await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage)).Count);
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
                Thread.Sleep(3.Seconds()); // blob storage needs 3 seconds

                var path = $"folder\\{Guid.NewGuid()}-nested.txt";
                Assert.True(await storage.SaveFileContentsAsync(path, "test"));
                fileInfo = await storage.GetFileInformationAsync(path);
                Assert.NotNull(fileInfo);
                Assert.True(fileInfo.Path.EndsWith("nested.txt", StringComparison.OrdinalIgnoreCase), "Incorrect file");
                Assert.True(fileInfo.Size > 0, "Incorrect file size");
                Assert.Equal(DateTimeKind.Utc, fileInfo.Created.Kind);
                // NOTE: File creation time might not be accurate: http://stackoverflow.com/questions/2109152/unbelievable-strange-file-creation-time-problem
                //Assert.True(fileInfo.Created > DateTime.MinValue, "File creation time should be newer than the start time.");
                //Assert.Equal(DateTimeKind.Utc, fileInfo.Modified.Kind);
                //Assert.True(startTime <= fileInfo.Created, $"File {path} created time {fileInfo.Modified:O} should be newer than the start time {startTime:O}.");

                //Thread.Sleep(3.Seconds()); // blob storage needs 3 seconds
                path = $"{Guid.NewGuid()}-test.txt";
                Assert.True(await storage.SaveFileContentsAsync(path, "test"));
                fileInfo = await storage.GetFileInformationAsync(path);
                Assert.NotNull(fileInfo);
                Assert.True(fileInfo.Path.EndsWith("test.txt", StringComparison.OrdinalIgnoreCase), "Incorrect file");
                Assert.True(fileInfo.Size > 0, "Incorrect file size");
                Assert.Equal(DateTimeKind.Utc, fileInfo.Created.Kind);
                //Assert.True(fileInfo.Created > DateTime.MinValue, "File creation time should be newer than the start time.");
                //Assert.Equal(DateTimeKind.Utc, fileInfo.Modified.Kind);
                //Assert.True(startTime <= fileInfo.Modified, $"File {path} modified time {fileInfo.Modified:O} should be newer than the start time {startTime:O}.");
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
                var file = (await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage)).Single();
                Assert.NotNull(file);
                Assert.Equal(path1, file.Path);
                var content = await storage.GetFileContentsAsync(path1);
                Assert.Equal("test", content);
                await storage.RenameFileAsync(path1, path2);
                Assert.Contains(await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage), f => f.Path == path2);
                await storage.DeleteFileAsync(path2);
                Assert.Empty(await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage));
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
                Assert.Single(await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage));

                Assert.True(await storage.SaveFileContentsAsync(path2, "test2"));
                Assert.True(await storage.RenameFileAsync(path2, @"archive\new.txt"));
                Assert.Equal("test2", await storage.GetFileContentsAsync(@"archive\new.txt"));
                Assert.Single(await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage));
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

                using (var stream = StreamHelper.ToStream("test data"))
                {
                    var result = await storage.SaveFileAsync(path, stream); // write
                    Assert.True(result);
                }

                Assert.Single(await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage));
                Assert.True(await storage.ExistsAsync(path));

                using (var stream = await storage.GetFileStreamAsync(path)) // read
                using (var reader = new StreamReader(stream))
                {
                    Assert.Equal("test data", await reader.ReadToEndAsync().AnyContext());
                }
            }
        }

        public virtual async Task CanSaveCsvFileAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var path = $"test-{Guid.NewGuid().ToString("N").Substring(10)}.csv";
                Assert.False(await storage.ExistsAsync(path));

                var data = new List<StubEntity>
                {
                    new StubEntity { FirstName = "John", LastName = RandomGenerator.GenerateString(4), Age = 100000 - 12.6M },
                    new StubEntity { FirstName = "John", LastName = RandomGenerator.GenerateString(4), Age = 11 }
                };

                var result = await storage.SaveFileCsvAsync(path, data, cultureInfo: new System.Globalization.CultureInfo("de-DE")); // write
                Assert.True(result);

                Assert.Single(await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage));
                Assert.True(await storage.ExistsAsync(path));

                var resultData = await storage.GetFileCsvAsync<IEnumerable<StubEntity>>(path);
                Assert.Equal(data[0].FirstName, resultData.FirstOrDefault()?.FirstName);
                Assert.Equal(data[0].LastName, resultData.FirstOrDefault()?.LastName);
                Assert.Equal(data[0].Timestamp.ToString("s"), resultData.FirstOrDefault()?.Timestamp.ToString("s"));
            }
        }

        public virtual async Task CanSaveCsvWithCustomHeaderFileAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var path = $"test-{Guid.NewGuid().ToString("N").Substring(10)}.csv";
                Assert.False(await storage.ExistsAsync(path));
                var headers = new Dictionary<string, string>
                {
                    {"FirstName", "New First name"},
                    {"LastName", "New Last name"},
                    {"Age", "Avg Age"},
                    {"Value", "Val"},
                    {"Timestamp", "created"}
                };

                var data = new List<StubEntity>
                {
                    new StubEntity { FirstName = "John", LastName = RandomGenerator.GenerateString(4), Age = 100000 - 12.6M },
                    new StubEntity { FirstName = "John", LastName = RandomGenerator.GenerateString(4), Age = 11 }
                };

                var result = await storage.SaveFileCsvAsync<IEnumerable<StubEntity>, StubEntity>(path, data, cultureInfo: new System.Globalization.CultureInfo("de-DE")/*, dateTimeFormat: "u"*/, headersMap: headers); // write
                Assert.True(result);

                Assert.Single(await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage));
                Assert.True(await storage.ExistsAsync(path));

                var resultData = await storage.GetFileCsvAsync<IEnumerable<StubEntity>>(path);
                Assert.Equal(data[0].FirstName, resultData.FirstOrDefault()?.FirstName);
                Assert.Equal(data[0].LastName, resultData.FirstOrDefault()?.LastName);
                Assert.Equal(data[0].Timestamp.ToString("s"), resultData.FirstOrDefault()?.Timestamp.ToString("s"));
            }
        }

        public virtual async Task CanSaveCsvDictionaryFileAsync()
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var path = $"test-{Guid.NewGuid().ToString("N").Substring(10)}.csv";
                Assert.False(await storage.ExistsAsync(path));

                var data = new List<Dictionary<string, string>>
                {
                    new Dictionary<string, string> { ["FirstName"] = "John1", ["LastName"] = "Doe1" },
                    new Dictionary<string, string> { ["FirstName"] = "John2", ["LastName"] = "Doe2" }
                };

                var result = await storage.SaveFileCsvAsync(path, data, cultureInfo: new System.Globalization.CultureInfo("de-DE")); // write
                Assert.True(result);

                Assert.Single(await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage));
                Assert.True(await storage.ExistsAsync(path));

                var resultData = await storage.GetFileCsvAsync<IEnumerable<Dictionary<string, string>>>(path);
                Assert.Equal("John1", data[0].Values.FirstOrDefault());
            }
        }

        public virtual async Task CanSaveFilesWithSerializerAsync(ISerializer serializer, string fileExtension)
        {
            await this.ResetAsync();

            var storage = this.GetStorage();
            if (storage == null)
            {
                return;
            }

            using (storage)
            {
                var path = $"entity-{Guid.NewGuid().ToString("N").Substring(10)}.{fileExtension}";
                Assert.False(await storage.ExistsAsync(path));

                var entity = new StubEntity
                {
                    FirstName = "John",
                    LastName = "Doe",
                    Age = 100000 - 12.6M
                };

                var saveResult = await storage.SaveFileObjectAsync(path, entity, serializer); // write
                Assert.True(saveResult);

                Assert.Single(await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage));
                Assert.True(await storage.ExistsAsync(path));

                var getResult = await storage.GetFileObjectAsync<StubEntity>(path, serializer); // read
                Assert.Equal("John", getResult.FirstName);
                Assert.Equal("Doe", getResult.LastName);
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
                Assert.Equal(2, (await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage)).Count);

                await storage.DeleteFilesAsync(@"x");
                Assert.Empty(await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage));
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
                Assert.Equal(2, (await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage)).Count);
                Assert.Single(await storage.GetFileInformationsAsync(limit: 1));
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\*")).Count);
                Assert.Single(await storage.GetFileInformationsAsync(@"x\nested\*"));

                await storage.DeleteFilesAsync(@"x\*");

                Assert.Empty(await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage));
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
                Assert.Equal(3, (await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage)).Count);
                Assert.Single(await storage.GetFileInformationsAsync(limit: 1));
                Assert.Equal(3, (await storage.GetFileInformationsAsync(@"x\*")).Count);
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\nested\*")).Count);
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\*.txt")).Count);

                await storage.DeleteFilesAsync(@"x\*.txt");

                Assert.Single(await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage));
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
                Assert.Equal(3, (await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage)).Count);
                Assert.Single(await storage.GetFileInformationsAsync(limit: 1));
                Assert.Equal(3, (await storage.GetFileInformationsAsync(@"x\*")).Count);
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\nested\*")).Count);
                Assert.Equal(2, (await storage.GetFileInformationsAsync(@"x\*.txt")).Count);

                await storage.DeleteFilesAsync(@"x\nested");

                Assert.Single(await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage));
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
                Assert.Equal(5, (await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage)).Count);
                Assert.Single(await storage.GetFileInformationsAsync(limit: 1));
                Assert.Equal(5, (await storage.GetFileInformationsAsync(@"x\*")).Count);
                Assert.Equal(3, (await storage.GetFileInformationsAsync(@"x\nested\*")).Count);
                Assert.Equal(3, (await storage.GetFileInformationsAsync(@"x\*.txt")).Count);

                await storage.DeleteFilesAsync(@"x\nested\*.txt");

                Assert.Equal(3, (await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage)).Count);
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
                var element = XElement.Parse("<user>Doe</user>");

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

        //public virtual async Task WillRespectStreamOffsetAsync()
        //{
        //    await this.ResetAsync();

        //    var storage = this.GetStorage();
        //    if (storage == null)
        //    {
        //        return;
        //    }

        //    using (storage)
        //    {
        //        var path = "doe.txt";
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            long offset;
        //            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true))
        //            {
        //                writer.AutoFlush = true;
        //                await writer.WriteAsync("John");
        //                offset = memoryStream.Position;
        //                await writer.WriteAsync("Doe");
        //            }

        //            memoryStream.Seek(offset, SeekOrigin.Begin);
        //            await storage.SaveFileAsync(path, memoryStream);
        //        }

        //        Assert.Equal("Doe", await storage.GetFileContentsAsync(path));
        //    }
        //}

        public virtual void CanUseDataDirectory()
        {
            const string DATA_DIRECTORY_QUEUE_FOLDER = @"|DataDirectory|\Queue";
#pragma warning disable CA2000 // Dispose objects before losing scope
            var storage = new FolderFileStorage(
                new FolderFileStorageOptions
                {
                    LoggerFactory = Substitute.For<ILoggerFactory>(),
                    Folder = DATA_DIRECTORY_QUEUE_FOLDER
                });
#pragma warning restore CA2000 // Dispose objects before losing scope
            Assert.NotNull(storage.Folder);
            Assert.NotEqual(DATA_DIRECTORY_QUEUE_FOLDER, storage.Folder);
            Assert.True(storage.Folder.EndsWith("Queue" + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase), storage.Folder);
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
                var files = (await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage)).ToList();
                if (files.Count > 0)
                {
                    await storage.DeleteFilesAsync(files);
                }

                Assert.Empty(await Naos.FileStorage.Domain.FileStorageExtensions.GetFileInformationsAsync(storage));
            }
        }

        protected virtual IFileStorage GetStorage()
        {
            return null;
        }

        public class StubEntity
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public decimal Age { get; set; }

            public long Value { get; set; } = long.MaxValue;

            public DateTime Timestamp { get; set; } = DateTime.UtcNow; //new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }
    }
}