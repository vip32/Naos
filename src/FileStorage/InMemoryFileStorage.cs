namespace Naos.FileStorage
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using Naos.FileStorage.Domain;
    using Naos.Foundation;

    public class InMemoryFileStorage : IFileStorage
    {
        private readonly InMemoryFileStorageOptions options;
        private readonly Dictionary<string, Tuple<FileInformation, byte[]>> storage = new Dictionary<string, Tuple<FileInformation, byte[]>>(StringComparer.OrdinalIgnoreCase);
        private readonly object @lock = new object();

        public InMemoryFileStorage()
            : this(new InMemoryFileStorageOptions())
        {
        }

        public InMemoryFileStorage(InMemoryFileStorageOptions options)
        {
            this.options = options ?? new InMemoryFileStorageOptions();
            this.Serializer = this.options.Serializer ?? DefaultSerializer.Create;

            this.MaxFileSize = options.MaxFileSize;
            this.MaxFiles = options.MaxFiles;
        }

        public InMemoryFileStorage(Builder<InMemoryFileStorageOptionsBuilder, InMemoryFileStorageOptions> optionsBuilder)
            : this(optionsBuilder(new InMemoryFileStorageOptionsBuilder()).Build())
        {
        }

        public long MaxFileSize { get; }

        public long MaxFiles { get; }

        public ISerializer Serializer { get; }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            lock (this.@lock)
            {
                if (!this.storage.ContainsKey(path))
                {
                    return Task.FromResult<Stream>(null);
                }

                return Task.FromResult<Stream>(new MemoryStream(this.storage[path].Item2));
            }
        }

        public async Task<FileInformation> GetFileInformationAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            return await this.ExistsAsync(path).AnyContext() ? this.storage[path].Item1 : null;
        }

        public Task<bool> ExistsAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            return Task.FromResult(this.storage.ContainsKey(path));
        }

        public Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNull(stream, nameof(stream));

            path = PathHelper.Normalize(path);
            var contents = ReadBytes(stream);
            if (contents.Length > this.MaxFileSize)
            {
                throw new NaosException(string.Format("File size {0} exceeds the maximum size of {1}.", contents.Length.Bytes().ToString("#.##"), this.MaxFileSize.Bytes().ToString("#.##")));
            }

            lock (this.@lock)
            {
                this.storage[path] = Tuple.Create(
                    new FileInformation
                    {
                        Path = path,
                        Name = path.SliceFromLast(Path.DirectorySeparatorChar.ToString()),
                        Created = DateTime.UtcNow,
                        Modified = DateTime.UtcNow,
                        Size = contents.Length
                    }, contents);

                if (this.storage.Count > this.MaxFiles)
                {
                    // storage full, remove oldest
                    this.storage.Remove(this.storage.OrderByDescending(kvp => kvp.Value.Item1.Created).First().Key);
                }
            }

            return Task.FromResult(true);
        }

        public Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(newPath, nameof(newPath));

            path = PathHelper.Normalize(path);
            newPath = PathHelper.Normalize(newPath);
            lock (this.@lock)
            {
                if (!this.storage.ContainsKey(path))
                {
                    return Task.FromResult(false);
                }

                this.storage[newPath] = this.storage[path];
                this.storage[newPath].Item1.Path = newPath;
                this.storage[newPath].Item1.Modified = DateTime.UtcNow;
                this.storage.Remove(path);
            }

            return Task.FromResult(true);
        }

        public Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(targetPath, nameof(targetPath));

            path = PathHelper.Normalize(path);
            targetPath = PathHelper.Normalize(targetPath);
            lock (this.@lock)
            {
                if (!this.storage.ContainsKey(path))
                {
                    return Task.FromResult(false);
                }

                this.storage[targetPath] = this.storage[path];
                this.storage[targetPath].Item1.Path = targetPath;
                this.storage[targetPath].Item1.Modified = DateTime.UtcNow;
            }

            return Task.FromResult(true);
        }

        public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            lock (this.@lock)
            {
                if (!this.storage.ContainsKey(path))
                {
                    return Task.FromResult(false);
                }

                this.storage.Remove(path);
            }

            return Task.FromResult(true);
        }

        public Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(searchPattern) || searchPattern == "*")
            {
                lock (this.@lock)
                {
                    this.storage.Clear();
                }

                return Task.FromResult(0);
            }

            searchPattern = PathHelper.Normalize(searchPattern);
            var count = 0;

            if (searchPattern[searchPattern.Length - 1] == Path.DirectorySeparatorChar)
            {
                searchPattern = $"{searchPattern}*";
            }
            else if (!searchPattern.EndsWith(Path.DirectorySeparatorChar + "*", StringComparison.OrdinalIgnoreCase) && !Path.HasExtension(searchPattern))
            {
                searchPattern = Path.Combine(searchPattern, "*");
            }

            var regex = new Regex("^" + Regex.Escape(searchPattern).Replace("\\*", ".*?") + "$");
            lock (this.@lock)
            {
                var keys = this.storage.Keys.Where(k => regex.IsMatch(k)).Select(k => this.storage[k].Item1).ToList();
                foreach (var key in keys)
                {
                    this.storage.Remove(key.Path);
                    count++;
                }
            }

            return Task.FromResult(count);
        }

        public async Task<PagedResults> GetFileInformationsAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default)
        {
            if (pageSize <= 0)
            {
                return PagedResults.EmptyResults;
            }

            if (searchPattern == null)
            {
                searchPattern = "*";
            }

            searchPattern = PathHelper.Normalize(searchPattern);

            var result = new PagedResults(() => Task.FromResult(this.GetFiles(searchPattern, 1, pageSize)));
            await result.NextPageAsync().AnyContext();
            return result;
        }

        public void Dispose()
        {
            this.storage?.Clear();
        }

        private static byte[] ReadBytes(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.Position = 0;
                stream.CopyTo(ms);
                stream.Position = stream.Length;
                return ms.ToArray();
            }
        }

        private NextPageResult GetFiles(string searchPattern, int page, int pageSize)
        {
            var list = new List<FileInformation>();
            var pagingLimit = pageSize;
            var skip = (page - 1) * pagingLimit;
            if (pagingLimit < int.MaxValue)
            {
                pagingLimit += 1;
            }

            var regex = new Regex("^" + Regex.Escape(searchPattern).Replace("\\*", ".*?") + "$");

            lock (this.@lock)
            {
                list.AddRange(this.storage.Keys.Where(k => regex.IsMatch(k)).Select(k => this.storage[k].Item1).Skip(skip).Take(pagingLimit).ToList());
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
