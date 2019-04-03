namespace Naos.Core.FileStorage.Infrastructure
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;

    public class FolderFileStorage : IFileStorage
    {
        private readonly FolderFileStorageOptions options;
        private readonly object @lock = new object();

        public FolderFileStorage(FolderFileStorageOptions options)
        {
            this.options = options ?? new FolderFileStorageOptions();
            this.Serializer = this.options.Serializer ?? DefaultSerializer.Create;

            var folder = PathHelper.ExpandPath(this.options.Folder);
            if (!Path.IsPathRooted(folder))
            {
                folder = Path.GetFullPath(folder);
            }

            var lastCharacter = folder[folder.Length - 1];
            if (!lastCharacter.Equals(Path.DirectorySeparatorChar) && !lastCharacter.Equals(Path.AltDirectorySeparatorChar))
            {
                folder += Path.DirectorySeparatorChar;
            }

            this.Folder = folder;
            Directory.CreateDirectory(folder);
        }

        public FolderFileStorage(Builder<FolderFileStorageOptionsBuilder, FolderFileStorageOptions> config)
            : this(config(new FolderFileStorageOptionsBuilder()).Build())
        {
        }

        public ISerializer Serializer { get; }

        public string Folder { get; }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            return Task.FromResult<Stream>(File.OpenRead(Path.Combine(this.Folder, path)));
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
                Path = info.FullName,
                Name = info.Name,
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

            path = Path.Combine(this.Folder, PathHelper.Normalize(path));
            using (var fileStream = this.CreateFileStream(path))
            {
                var offset = stream.Position;
                stream.Position = 0;
                await stream.CopyToAsync(fileStream).AnyContext();
                stream.Seek(offset, SeekOrigin.Begin);

                return true;
            }
        }

        public Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(newPath, nameof(newPath));

            path = PathHelper.Normalize(path);
            newPath = PathHelper.Normalize(newPath);
            lock (this.@lock)
            {
                var directory = Path.GetDirectoryName(newPath);
                if (directory != null)
                {
                    Directory.CreateDirectory(Path.Combine(this.Folder, directory));
                }

                var oldFullPath = Path.Combine(this.Folder, path);
                var newFullPath = Path.Combine(this.Folder, newPath);
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

            return Task.FromResult(true);
        }

        public Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(targetPath, nameof(targetPath));

            path = PathHelper.Normalize(path);
            lock (this.@lock)
            {
                var directory = Path.GetDirectoryName(targetPath);
                if (directory != null)
                {
                    Directory.CreateDirectory(Path.Combine(this.Folder, directory));
                }

                File.Copy(Path.Combine(this.Folder, path), Path.Combine(this.Folder, targetPath));
            }

            return Task.FromResult(true);
        }

        public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            File.Delete(Path.Combine(this.Folder, path));
            return Task.FromResult(true);
        }

        public Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            if (searchPattern == null || string.IsNullOrEmpty(searchPattern) || searchPattern == "*")
            {
                Directory.Delete(this.Folder, true);
                return Task.FromResult(0);
            }

            searchPattern = PathHelper.Normalize(searchPattern);
            var count = 0;

            var path = Path.Combine(this.Folder, searchPattern);
            if (path[path.Length - 1] == Path.DirectorySeparatorChar || path.EndsWith(Path.DirectorySeparatorChar + "*"))
            {
                var directory = Path.GetDirectoryName(path);
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

            foreach (var file in Directory.EnumerateFiles(this.Folder, searchPattern, SearchOption.AllDirectories))
            {
                File.Delete(file);
                count++;
            }

            return Task.FromResult(count);
        }

        public async Task<PagedResults> GetFileInformationsAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default)
        {
            if (pageSize <= 0)
            {
                return PagedResults.EmptyResults;
            }

            if (searchPattern == null || string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = "*";
            }

            searchPattern = PathHelper.Normalize(searchPattern);

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

            var directory = Path.GetDirectoryName(filePath);
            if (directory != null)
            {
                Directory.CreateDirectory(directory);
            }

            return File.Create(filePath);
        }

        private NextPageResult GetFiles(string searchPattern, int page, int pageSize)
        {
            var list = new List<FileInformation>();
            var pagingLimit = pageSize;
            var skip = (page - 1) * pagingLimit;
            if (pagingLimit < int.MaxValue)
            {
                pagingLimit = pagingLimit + 1;
            }

            foreach (var path in Directory.EnumerateFiles(this.Folder, searchPattern, SearchOption.AllDirectories).Skip(skip).Take(pagingLimit))
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

            var hasMore = false;
            if (list.Count == pagingLimit)
            {
                hasMore = true;
                list.RemoveAt(pagingLimit);
            }

            return new NextPageResult { Success = true, HasMore = hasMore, Files = list, NextPageFunc = () => Task.FromResult(this.GetFiles(searchPattern, page + 1, pageSize)) };
        }
    }
}
