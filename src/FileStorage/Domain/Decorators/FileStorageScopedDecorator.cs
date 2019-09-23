namespace Naos.FileStorage.Domain
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Foundation;

    public class FileStorageScopedDecorator : IFileStorage
    {
        private readonly string pathPrefix;

        public FileStorageScopedDecorator(string scope, IFileStorage decoratee)
        {
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.Decoratee = decoratee;
            this.Scope = scope?.Trim();
            this.pathPrefix = this.Scope != null ? string.Concat(this.Scope, "/") : string.Empty;

            this.pathPrefix = this.pathPrefix
                .Replace("{environment}", Environment.GetEnvironmentVariable(EnvironmentKeys.Environment)?.ToLower());
        }

        public ISerializer Serializer => this.Decoratee.Serializer;

        private IFileStorage Decoratee { get; }

        private string Scope { get; }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            return this.Decoratee.GetFileStreamAsync(string.Concat(this.UpdatePathPrefix(this.pathPrefix), path), cancellationToken);
        }

        public async Task<FileInformation> GetFileInformationAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            var file = await this.Decoratee.GetFileInformationAsync(string.Concat(this.UpdatePathPrefix(this.pathPrefix), path)).AnyContext();
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

            return this.Decoratee.SaveFileAsync(string.Concat(this.UpdatePathPrefix(this.pathPrefix), path), stream, cancellationToken);
        }

        public Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(newPath, nameof(newPath));

            return this.Decoratee.RenameFileAsync(string.Concat(this.UpdatePathPrefix(this.pathPrefix), path), string.Concat(this.pathPrefix, newPath), cancellationToken);
        }

        public Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(targetPath, nameof(targetPath));

            return this.Decoratee.CopyFileAsync(string.Concat(this.UpdatePathPrefix(this.pathPrefix), path), string.Concat(this.pathPrefix, targetPath), cancellationToken);
        }

        public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            return this.Decoratee.DeleteFileAsync(string.Concat(this.UpdatePathPrefix(this.pathPrefix), path), cancellationToken);
        }

        public Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            return this.Decoratee.DeleteFilesAsync(string.Concat(this.UpdatePathPrefix(this.pathPrefix), searchPattern), cancellationToken);
        }

        public async Task<PagedResults> GetFileInformationsAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default)
        {
            if (pageSize <= 0)
            {
                return PagedResults.EmptyResults;
            }

            var unscopedResult = await this.Decoratee.GetFileInformationsAsync(pageSize, string.Concat(this.UpdatePathPrefix(this.pathPrefix), searchPattern), cancellationToken).AnyContext();
            foreach (var file in unscopedResult.Files)
            {
                file.Path = file.Path.Substring(this.UpdatePathPrefix(this.pathPrefix).Length);
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

        private string UpdatePathPrefix(string path)
        {
            var dateTime = DateTime.UtcNow;
            return path
                .Replace("{yyyy}", dateTime.Year.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{MM}", dateTime.Month.ToString().PadLeft(2, '0'), StringComparison.OrdinalIgnoreCase)
                .Replace("{dd}", dateTime.Day.ToString().PadLeft(2, '0'), StringComparison.OrdinalIgnoreCase);
        }
    }
}
