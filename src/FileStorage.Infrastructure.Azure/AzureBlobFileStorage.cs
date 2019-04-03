namespace Naos.Core.FileStorage.Infrastructure
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
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;

    public class AzureBlobFileStorage : IFileStorage
    {
        private readonly AzureBlobFileStorageOptions options;
        private CloudBlobContainer container;
        private bool initialized;

        public AzureBlobFileStorage(AzureBlobFileStorageOptions options)
        {
            this.options = options ?? new AzureBlobFileStorageOptions();
            this.Serializer = this.options.Serializer ?? DefaultSerializer.Create;
        }

        public AzureBlobFileStorage(Builder<AzureBlobFileStorageOptionsBuilder, AzureBlobFileStorageOptions> optionsBuilder)
            : this(optionsBuilder(new AzureBlobFileStorageOptionsBuilder()).Build())
        {
        }

        public ISerializer Serializer { get; }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.Initialize();
            var blockBlob = this.container.GetBlockBlobReference(path);
            try
            {
                return await blockBlob.OpenReadAsync(null, null, null, cancellationToken).AnyContext();
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 404)
                {
                    return null;
                }

                throw;
            }
        }

        public async Task<FileInformation> GetFileInformationAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.Initialize();
            var blob = this.container.GetBlockBlobReference(path);
            try
            {
                await blob.FetchAttributesAsync().AnyContext();
                return blob.ToFileInfo();
            }
            catch (Exception)
            {
                // TODO: log?
                return null;
            }
        }

        public Task<bool> ExistsAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.Initialize();
            var blockBlob = this.container.GetBlockBlobReference(path);
            return blockBlob.ExistsAsync();
        }

        public async Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNull(stream, nameof(stream));

            this.Initialize();
            var offset = stream.Position;
            stream.Position = 0;
            await this.container.GetBlockBlobReference(path)
                .UploadFromStreamAsync(stream, null, null, null, cancellationToken).AnyContext();
            stream.Seek(offset, SeekOrigin.Begin);

            return true;
        }

        public async Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(newPath, nameof(newPath));

            this.Initialize();
            var oldBlob = this.container.GetBlockBlobReference(path);
            if (!(await this.CopyFileAsync(path, newPath, cancellationToken).AnyContext()))
            {
                return false;
            }

            return await oldBlob.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, null, null, cancellationToken).AnyContext();
        }

        public async Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(targetPath, nameof(targetPath));

            this.Initialize();
            var oldBlob = this.container.GetBlockBlobReference(path);
            var newBlob = this.container.GetBlockBlobReference(targetPath);

            await newBlob.StartCopyAsync(oldBlob, null, null, null, null, cancellationToken).AnyContext();
            while (newBlob.CopyState.Status == CopyStatus.Pending)
            {
                await Task.Delay(50, cancellationToken).AnyContext();
            }

            return newBlob.CopyState.Status == CopyStatus.Success;
        }

        public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.Initialize();
            return this.container.GetBlockBlobReference(path)
                .DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, null, null, cancellationToken);
        }

        public async Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            this.Initialize();
            var files = await this.GetFileListAsync(searchPattern, cancellationToken: cancellationToken).AnyContext();
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
            if (pageSize <= 0)
            {
                return PagedResults.EmptyResults;
            }

            var result = new PagedResults(() => this.GetFiles(searchPattern, 1, pageSize, cancellationToken));
            await result.NextPageAsync().AnyContext();
            return result;
        }

        public void Dispose()
        {
        }

        private async Task<IEnumerable<FileInformation>> GetFileListAsync(
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

            prefix = prefix ?? string.Empty;

            BlobContinuationToken continuationToken = null;
            var blobs = new List<CloudBlockBlob>();
            do
            {
                var listingResult = await this.container.ListBlobsSegmentedAsync(prefix, true, BlobListingDetails.Metadata, limit, continuationToken, null, null, cancellationToken).AnyContext();
                continuationToken = listingResult.ContinuationToken;

                // TODO: Implement paging
                blobs.AddRange(listingResult.Results.OfType<CloudBlockBlob>().Matches(patternRegex));
            }
            while (continuationToken != null && blobs.Count < limit.GetValueOrDefault(int.MaxValue));

            if (limit.HasValue)
            {
                blobs = blobs.Take(limit.Value).ToList();
            }

            return blobs.Select(blob => blob.ToFileInfo());
        }

        private async Task<NextPageResult> GetFiles(string searchPattern, int page, int pageSize, CancellationToken cancellationToken)
        {
            var pagingLimit = pageSize;
            var skip = (page - 1) * pagingLimit;
            if (pagingLimit < int.MaxValue)
            {
                pagingLimit++;
            }

            var list = (await this.GetFileListAsync(searchPattern, pagingLimit, skip, cancellationToken).AnyContext()).ToList();
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
                NextPageFunc = () => this.GetFiles(searchPattern, page + 1, pageSize, cancellationToken)
            };
        }

        private void Initialize()
        {
            if (!this.initialized)
            {
                this.container = CloudStorageAccount.Parse(this.options.ConnectionString)
                    .CreateCloudBlobClient()
                    .GetContainerReference(this.options.ContainerName);
                this.container.CreateIfNotExistsAsync().GetAwaiter().GetResult();
                this.initialized = true;
            }
        }
    }
}