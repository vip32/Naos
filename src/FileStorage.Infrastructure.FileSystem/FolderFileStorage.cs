namespace Naos.Core.FileStorage.Infrastructure.FileSystem
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;
    using Naos.Core.FileStorage.Domain;

    public class FolderFileStorage : IFileStorage
    {
        private readonly ILogger<FolderFileStorage> logger;
        private readonly object @lock = new object();

        public FolderFileStorage(ILogger<FolderFileStorage> logger, FolderFileStorageOptions options)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            options = options ?? new FolderFileStorageOptions();
            this.Serializer = options.Serializer ?? DefaultSerializer.Instance;

            var folder = PathHelper.ExpandPath(options.Folder);
            if (!Path.IsPathRooted(folder))
            {
                folder = Path.GetFullPath(folder);
            }

            char lastCharacter = folder[folder.Length - 1];
            if (!lastCharacter.Equals(Path.DirectorySeparatorChar) && !lastCharacter.Equals(Path.AltDirectorySeparatorChar))
            {
                folder += Path.DirectorySeparatorChar;
            }

            this.Folder = folder;
            Directory.CreateDirectory(folder);
        }

        public ISerializer Serializer { get; }

        public string Folder { get; }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            try
            {
                return Task.FromResult<Stream>(File.OpenRead(Path.Combine(this.Folder, path)));
            }
            catch (IOException ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                if (this.logger.IsEnabled(LogLevel.Trace))
                {
                    this.logger.LogTrace(ex, "Error trying to get file stream: {Path}", path);
                }

                return Task.FromResult<Stream>(null);
            }
        }

        public Task<FileInformation> GetFileInformationAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            var info = new FileInfo(Path.Combine(this.Folder, path));
            if (!info.Exists)
            {
                return Task.FromResult<FileInformation>(null);
            }

            return Task.FromResult(new FileInformation
            {
                Path = path.Replace(this.Folder, string.Empty),
                Created = info.CreationTimeUtc,
                Modified = info.LastWriteTimeUtc,
                Size = info.Length
            });
        }

        public Task<bool> ExistsAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            return Task.FromResult(File.Exists(Path.Combine(this.Folder, path)));
        }

        public async Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNull(stream, nameof(stream));

            path = PathHelper.Normalize(path);
            string file = Path.Combine(this.Folder, path);
            try
            {
                using (var fileStream = this.CreateFileStream(file))
                {
                    await stream.CopyToAsync(fileStream).AnyContext();
                    return true;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error trying to save file: {Path}", path);
                return false;
            }
        }

        public Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(newPath, nameof(newPath));

            path = path.NormalizePath();
            newPath = newPath.NormalizePath();
            try
            {
                lock (this.@lock)
                {
                    string directory = Path.GetDirectoryName(newPath);
                    if (directory != null)
                    {
                        Directory.CreateDirectory(Path.Combine(this.Folder, directory));
                    }

                    string oldFullPath = Path.Combine(this.Folder, path);
                    string newFullPath = Path.Combine(this.Folder, newPath);
                    try
                    {
                        File.Move(oldFullPath, newFullPath);
                    }
                    catch (IOException)
                    {
                        File.Delete(newFullPath);
                        File.Move(oldFullPath, newFullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error trying to rename file {Path} to {NewPath}.", path, newPath);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(targetPath, nameof(targetPath));

            path = path.NormalizePath();
            try
            {
                lock (this.@lock)
                {
                    string directory = Path.GetDirectoryName(targetPath);
                    if (directory != null)
                    {
                        Directory.CreateDirectory(Path.Combine(this.Folder, directory));
                    }

                    File.Copy(Path.Combine(this.Folder, path), Path.Combine(this.Folder, targetPath));
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error trying to copy file {Path} to {TargetPath}.", path, targetPath);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = path.NormalizePath();
            try
            {
                File.Delete(Path.Combine(this.Folder, path));
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                this.logger.LogDebug(ex, "Error trying to delete file: {Path}.", path);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            if (searchPattern == null || string.IsNullOrEmpty(searchPattern) || searchPattern == "*")
            {
                this.logger.LogInformation($"{{LogKey:l}} delete folder: {this.Folder}", LogEventKeys.FileStorage);
                Directory.Delete(this.Folder, true);
                return Task.FromResult(0);
            }

            searchPattern = searchPattern.NormalizePath();
            int count = 0;

            string path = Path.Combine(this.Folder, searchPattern);
            if (path[path.Length - 1] == Path.DirectorySeparatorChar || path.EndsWith(Path.DirectorySeparatorChar + "*"))
            {
                string directory = Path.GetDirectoryName(path);
                if (Directory.Exists(directory))
                {
                    count += Directory.EnumerateFiles(directory, "*,*", SearchOption.AllDirectories).Count();
                    Directory.Delete(directory, true);
                    return Task.FromResult(count);
                }
            }
            else if (Directory.Exists(path))
            {
                count += Directory.EnumerateFiles(path, "*,*", SearchOption.AllDirectories).Count();
                Directory.Delete(path, true);
                return Task.FromResult(count);
            }

            foreach (string file in Directory.EnumerateFiles(this.Folder, searchPattern, SearchOption.AllDirectories))
            {
                this.logger.LogInformation($"{{LogKey:l}} delete file: {file}", LogEventKeys.FileStorage);
                File.Delete(file);
                count++;
            }

            return Task.FromResult(count);
        }

        public async Task<PagedResults> GetPagedFileListAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default)
        {
            if (pageSize <= 0)
            {
                return PagedResults.EmptyResults;
            }

            if (searchPattern == null || string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = "*";
            }

            searchPattern = searchPattern.NormalizePath();

            var list = new List<FileInformation>();
            if (!Directory.Exists(Path.GetDirectoryName(Path.Combine(this.Folder, searchPattern))))
            {
                return PagedResults.EmptyResults;
            }

            var result = new PagedResults(() => Task.FromResult(this.GetFiles(searchPattern, 1, pageSize)));
            await result.NextPageAsync().AnyContext();
            return result;
        }

        public void Dispose()
        {
        }

        private Stream CreateFileStream(string filePath)
        {
            try
            {
                return File.Create(filePath);
            }
            catch (DirectoryNotFoundException)
            {
                // created below
            }

            string directory = Path.GetDirectoryName(filePath);
            if (directory != null)
            {
                this.logger.LogInformation($"{{LogKey:l}} create directory: {directory}", LogEventKeys.FileStorage);
                Directory.CreateDirectory(directory);
            }

            return File.Create(filePath);
        }

        private NextPageResult GetFiles(string searchPattern, int page, int pageSize)
        {
            var list = new List<FileInformation>();
            int pagingLimit = pageSize;
            int skip = (page - 1) * pagingLimit;
            if (pagingLimit < int.MaxValue)
            {
                pagingLimit = pagingLimit + 1;
            }

            foreach (string path in Directory.EnumerateFiles(this.Folder, searchPattern, SearchOption.AllDirectories).Skip(skip).Take(pagingLimit))
            {
                var info = new FileInfo(path);
                if (!info.Exists)
                {
                    continue;
                }

                list.Add(new FileInformation
                {
                    Path = info.FullName.Replace(this.Folder, string.Empty),
                    Created = info.CreationTimeUtc,
                    Modified = info.LastWriteTimeUtc,
                    Size = info.Length
                });
            }

            bool hasMore = false;
            if (list.Count == pagingLimit)
            {
                hasMore = true;
                list.RemoveAt(pagingLimit);
            }

            return new NextPageResult { Success = true, HasMore = hasMore, Files = list, NextPageFunc = () => Task.FromResult(this.GetFiles(searchPattern, page + 1, pageSize)) };
        }
    }
}
