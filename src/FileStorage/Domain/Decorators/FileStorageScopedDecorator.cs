namespace Naos.Core.FileStorage.Domain
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;

    public class FileStorageScopedDecorator : IFileStorage
    {
        private readonly string pathPrefix;

        public FileStorageScopedDecorator(IFileStorage decoratee, string scope)
        {
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.Decoratee = decoratee;
            this.Scope = scope?.Trim();
            this.pathPrefix = this.Scope != null ? string.Concat(this.Scope, "/") : string.Empty;
        }

        public ISerializer Serializer => this.Decoratee.Serializer;

        private IFileStorage Decoratee { get; }

        private string Scope { get; }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            return this.Decoratee.GetFileStreamAsync(string.Concat(this.pathPrefix, path), cancellationToken);
        }

        public async Task<FileInformation> GetFileInformationAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            var file = await this.Decoratee.GetFileInformationAsync(string.Concat(this.pathPrefix, path)).AnyContext();
            if (file != null)
            {
                file.Path = file.Path.Substring(this.pathPrefix.Length);
            }

            return file;
        }

        public Task<bool> ExistsAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            return this.Decoratee.ExistsAsync(string.Concat(this.pathPrefix, path));
        }

        public Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNull(stream, nameof(stream));

            return this.Decoratee.SaveFileAsync(string.Concat(this.pathPrefix, path), stream, cancellationToken);
        }

        public Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(newPath, nameof(newPath));

            return this.Decoratee.RenameFileAsync(string.Concat(this.pathPrefix, path), string.Concat(this.pathPrefix, newPath), cancellationToken);
        }

        public Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(targetPath, nameof(targetPath));

            return this.Decoratee.CopyFileAsync(string.Concat(this.pathPrefix, path), string.Concat(this.pathPrefix, targetPath), cancellationToken);
        }

        public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            return this.Decoratee.DeleteFileAsync(string.Concat(this.pathPrefix, path), cancellationToken);
        }

        public Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            return this.Decoratee.DeleteFilesAsync(string.Concat(this.pathPrefix, searchPattern), cancellationToken);
        }

        public async Task<PagedResults> GetPagedFileListAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default)
        {
            if (pageSize <= 0)
            {
                return PagedResults.EmptyResults;
            }

            var unscopedResult = await this.Decoratee.GetPagedFileListAsync(pageSize, string.Concat(this.pathPrefix, searchPattern), cancellationToken).AnyContext();
            foreach (var file in unscopedResult.Files)
            {
                file.Path = file.Path.Substring(this.pathPrefix.Length);
            }

            return new PagedResults(unscopedResult.Files, unscopedResult.HasMore, () => this.NextPage(unscopedResult));
        }

        public void Dispose()
        {
            this.Decoratee?.Dispose();
        }

        private async Task<NextPageResult> NextPage(PagedResults result)
        {
            var success = await result.NextPageAsync().AnyContext();
            foreach (var file in result.Files)
            {
                file.Path = file.Path.Substring(this.pathPrefix.Length);
            }

            return new NextPageResult { Success = success, HasMore = result.HasMore, Files = result.Files, NextPageFunc = () => this.NextPage(result) };
        }
    }
}
