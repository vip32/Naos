namespace Naos.Core.FileStorage.Infrastructure.Azure
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;
    using Naos.Core.FileStorage.Domain;

    public class AzureFileStorage : IFileStorage
    {
        private readonly ILogger<AzureFileStorage> logger;
        private readonly CloudBlobContainer container;
        private readonly ISerializer serializer;

        public AzureFileStorage(ILogger<AzureFileStorage> logger, AzureFileStorageOptions options)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            options = options ?? new AzureFileStorageOptions();
            this.Serializer = options.Serializer ?? DefaultSerializer.Instance;

            this.container = CloudStorageAccount.Parse(options.ConnectionString)
                .CreateCloudBlobClient()
                .GetContainerReference(options.ContainerName);
            this.container.CreateIfNotExistsAsync().GetAwaiter().GetResult();
            this.serializer = options.Serializer ?? DefaultSerializer.Instance;
        }

        public AzureFileStorage(ILogger<AzureFileStorage> logger, Builder<AzureFileStorageOptionsBuilder, AzureFileStorageOptions> config)
            : this(logger, config(new AzureFileStorageOptionsBuilder()).Build())
        {
        }

        public ISerializer Serializer { get; }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

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

            var blockBlob = this.container.GetBlockBlobReference(path);
            return blockBlob.ExistsAsync();
        }

        public async Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNull(stream, nameof(stream));

            var blockBlob = this.container.GetBlockBlobReference(path);
            await blockBlob.UploadFromStreamAsync(stream, null, null, null, cancellationToken).AnyContext();

            return true;
        }

        public async Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(newPath, nameof(newPath));

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

            var blockBlob = this.container.GetBlockBlobReference(path);
            return blockBlob.DeleteIfExistsAsync(DeleteSnapshotsOption.None, null, null, null, cancellationToken);
        }

        public async Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            var files = await this.GetFileListAsync(searchPattern, cancellationToken: cancellationToken).AnyContext();
            int count = 0;

            foreach (var file in files) // batch?
            {
                this.logger.LogInformation($"{{LogKey:l}} delete file: {file.Path}", LogEventKeys.FileStorage);
                await this.DeleteFileAsync(file.Path, cancellationToken).AnyContext();
                count++;
            }

            return count;
        }

        public async Task<PagedResults> GetPagedFileListAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default)
        {
            if (pageSize <= 0)
            {
                return PagedResults.EmptyResults;
            }

            var result = new PagedResults(() => this.GetFiles(searchPattern, 1, pageSize, cancellationToken));
            await result.NextPageAsync().AnyContext();
            return result;
        }

        public async Task<IEnumerable<FileInformation>> GetFileListAsync(
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
            string prefix = searchPattern;
            Regex patternRegex = null;
            int wildcardPos = searchPattern?.IndexOf('*') ?? -1;
            if (searchPattern != null && wildcardPos >= 0)
            {
                patternRegex = new Regex("^" + Regex.Escape(searchPattern).Replace("\\*", ".*?") + "$");
                int slashPos = searchPattern.LastIndexOf('/');
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
                blobs.AddRange(listingResult.Results.OfType<CloudBlockBlob>().MatchesPattern(patternRegex));
            }
            while (continuationToken != null && blobs.Count < limit.GetValueOrDefault(int.MaxValue));

            if (limit.HasValue)
            {
                blobs = blobs.Take(limit.Value).ToList();
            }

            return blobs.Select(blob => blob.ToFileInfo());
        }

        public void Dispose()
        {
        }

        private async Task<NextPageResult> GetFiles(string searchPattern, int page, int pageSize, CancellationToken cancellationToken)
        {
            int pagingLimit = pageSize;
            int skip = (page - 1) * pagingLimit;
            if (pagingLimit < int.MaxValue)
            {
                pagingLimit++;
            }

            var list = (await this.GetFileListAsync(searchPattern, pagingLimit, skip, cancellationToken).AnyContext()).ToList();
            bool hasMore = false;
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
    }
}