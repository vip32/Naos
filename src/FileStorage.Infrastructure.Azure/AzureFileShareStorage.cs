namespace Naos.FileStorage.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.File;
    using Naos.FileStorage.Domain;
    using Naos.Foundation;

    public class AzureFileShareStorage : IFileStorage
    {
        private readonly AzureFileShareStorageOptions options;
        private CloudFileClient client;
        private CloudFileShare share;
        private bool initialized;

        public AzureFileShareStorage(AzureFileShareStorageOptions options)
        {
            EnsureArg.IsNotNull(options, nameof(options));
            EnsureArg.IsNotNullOrEmpty(options.ConnectionString, nameof(options.ConnectionString));
            EnsureArg.IsNotNullOrEmpty(options.ShareName, nameof(options.ShareName));

            this.options = options;
            this.Serializer = this.options.Serializer ?? DefaultSerializer.Create;
        }

        public AzureFileShareStorage(Builder<AzureFileShareStorageOptionsBuilder, AzureFileShareStorageOptions> optionsBuilder)
            : this(optionsBuilder(new AzureFileShareStorageOptionsBuilder()).Build())
        {
        }

        public ISerializer Serializer { get; }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.Initialize();
            var folder = this.share.GetRootDirectoryReference();
            var pathParts = path.Split('/');
            var fileName = pathParts[pathParts.Length - 1];

            for (var i = 0; i < pathParts.Length - 2; i++)
            {
                folder = folder.GetDirectoryReference(pathParts[i]);
                if (!await folder.ExistsAsync().AnyContext())
                {
                    return null;
                }
            }

            var fileRef = folder.GetFileReference(fileName);
            if (!await fileRef.ExistsAsync().AnyContext())
            {
                return null;
            }

            return await fileRef.OpenReadAsync().AnyContext();
        }

        public async Task<FileInformation> GetFileInformationAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.Initialize();
            var folder = this.share.GetRootDirectoryReference();
            var pathParts = path.Split('/');
            var fileName = pathParts[pathParts.Length - 1];

            for (var i = 0; i < pathParts.Length - 2; i++)
            {
                folder = folder.GetDirectoryReference(pathParts[i]);
                if (!await folder.ExistsAsync().ConfigureAwait(false))
                {
                    return null;
                }
            }

            var fileRef = folder.GetFileReference(fileName);
            return fileRef.ToFileInfo();
        }

        public async Task<bool> ExistsAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.Initialize();
            var folder = this.share.GetRootDirectoryReference();
            var pathParts = path.Split('/');
            var fileName = pathParts[pathParts.Length - 1];

            for (var i = 0; i < pathParts.Length - 2; i++)
            {
                folder = folder.GetDirectoryReference(pathParts[i]);
                if (!await folder.ExistsAsync().ConfigureAwait(false))
                {
                    return false;
                }
            }

            var fileRef = folder.GetFileReference(fileName);

            return await fileRef.ExistsAsync().ConfigureAwait(false);
        }

        public async Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNull(stream, nameof(stream));

            this.Initialize();
            var folder = this.share.GetRootDirectoryReference();

            var offset = stream.Position;
            stream.Position = 0;
            var pathParts = path.Split('/');
            var fileName = pathParts[pathParts.Length - 1];

            for (var i = 0; i < pathParts.Length - 2; i++)
            {
                folder = folder.GetDirectoryReference(pathParts[i]);

                await folder.CreateIfNotExistsAsync().ConfigureAwait(false);
            }

            var fileRef = folder.GetFileReference(fileName);

            await fileRef.UploadFromStreamAsync(stream).ConfigureAwait(false);
            stream.Seek(offset, SeekOrigin.Begin);

            return true;
        }

        public Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException(); // https://www.codementor.io/tips/1374027784/programmatically-net-renaming-an-azure-file-or-directory-using-file-not-blob-storage
        }

        public async Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(targetPath, nameof(targetPath));

            var file = await this.GetFileReferenceAsync(path).AnyContext();
            var targetFile = await this.GetFileReferenceAsync(targetPath).AnyContext();

            await targetFile.StartCopyAsync(file).AnyContext();
            while (targetFile.CopyState.Status == CopyStatus.Pending) // todo: really wait?
            {
                await Task.Delay(50, cancellationToken).AnyContext();
            }

            return targetFile.CopyState.Status == CopyStatus.Success;
        }

        public async Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.Initialize();
            var folder = this.share.GetRootDirectoryReference();

            var pathParts = path.Split('/');
            var fileName = pathParts[pathParts.Length - 1];

            for (var i = 0; i < pathParts.Length - 2; i++)
            {
                folder = folder.GetDirectoryReference(pathParts[i]);
                if (!await folder.ExistsAsync().ConfigureAwait(false))
                {
                    return false;
                }
            }

            var fileRef = folder.GetFileReference(fileName);

            return await fileRef.DeleteIfExistsAsync().ConfigureAwait(false);
        }

        public async Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            this.Initialize();
            var folder = this.share.GetRootDirectoryReference();

            var files = await this.GetFileListAsync(folder, searchPattern, cancellationToken: cancellationToken).AnyContext();
            var count = 0;

            foreach (var file in files) // batch?
            {
                await this.DeleteFileAsync(file.Path, cancellationToken).AnyContext();
                count++;
            }

            return count;
        }

        public async Task<PagedResults> GetFileInformationsAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default)
        {
            this.Initialize();
            var folder = this.share.GetRootDirectoryReference();

            if (pageSize <= 0)
            {
                return PagedResults.EmptyResults;
            }

            var result = new PagedResults(() => this.GetFiles(folder, searchPattern, 1, pageSize, cancellationToken));
            await result.NextPageAsync().AnyContext();
            return result;
        }

        public void Dispose()
        {
        }

        private async Task<CloudFile> GetFileReferenceAsync(string path)
        {
            this.Initialize();
            var folder = this.share.GetRootDirectoryReference();
            var pathParts = path.Split('/');
            var fileName = pathParts[pathParts.Length - 1];

            for (var i = 0; i < pathParts.Length - 2; i++)
            {
                folder = folder.GetDirectoryReference(pathParts[i]);
                if (!await folder.ExistsAsync().ConfigureAwait(false))
                {
                    return null;
                }
            }

            return folder.GetFileReference(fileName);
        }

        private async Task<IEnumerable<FileInformation>> GetFileListAsync(
            CloudFileDirectory folder,
            string searchPattern = null,
            int? limit = null,
            int? skip = null,
            CancellationToken cancellationToken = default)
        {
            if (limit.HasValue && limit.Value <= 0)
            {
                return new List<FileInformation>();
            }

            searchPattern = searchPattern?.Replace('\\', '/');
            var prefix = searchPattern;
            Regex patternRegex = null;
            var wildcardPos = searchPattern?.IndexOf('*') ?? -1;
            if (searchPattern != null && wildcardPos >= 0)
            {
                patternRegex = new Regex("^" + Regex.Escape(searchPattern).Replace("\\*", ".*?") + "$");
                var slashPos = searchPattern.LastIndexOf('/');
                prefix = slashPos >= 0 ? searchPattern.Substring(0, slashPos) : string.Empty;
            }

            prefix ??= string.Empty;

            FileContinuationToken continuationToken = null;
            var files = new List<CloudFile>();
            do
            {
                var listingResult = await folder.ListFilesAndDirectoriesSegmentedAsync(prefix, limit, continuationToken, null, null, cancellationToken).AnyContext();
                continuationToken = listingResult.ContinuationToken;

                // TODO: Implement paging
                files.AddRange(listingResult.Results.OfType<CloudFile>().Matches(patternRegex));
            }
            while (continuationToken != null && files.Count < (limit ?? int.MaxValue));

            if (limit.HasValue)
            {
                files = files.Take(limit.Value).ToList();
            }

            return files.Select(file => file.ToFileInfo());
        }

        private async Task<NextPageResult> GetFiles(CloudFileDirectory folder, string searchPattern, int page, int pageSize, CancellationToken cancellationToken)
        {
            var pagingLimit = pageSize;
            var skip = (page - 1) * pagingLimit;
            if (pagingLimit < int.MaxValue)
            {
                pagingLimit++;
            }

            var list = (await this.GetFileListAsync(folder, searchPattern, pagingLimit, skip, cancellationToken).AnyContext()).ToList();
            var hasMore = false;
            if (list.Count == pagingLimit)
            {
                hasMore = true;
                list.RemoveAt(pagingLimit);
            }

            return new NextPageResult
            {
                Success = true,
                HasMore = hasMore,
                Files = list,
                NextPageFunc = () => this.GetFiles(folder, searchPattern, page + 1, pageSize, cancellationToken)
            };
        }

        private void Initialize()
        {
            if (!this.initialized)
            {
                this.client = CloudStorageAccount.Parse(this.options.ConnectionString)
                    .CreateCloudFileClient();
                this.share = this.client.GetShareReference(this.options.ShareName);
                this.initialized = true;
            }
        }
    }
}