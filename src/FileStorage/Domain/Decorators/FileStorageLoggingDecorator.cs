namespace Naos.FileStorage.Domain
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Humanizer;
    using Microsoft.Extensions.Logging;
    using Naos.Foundation;

    public class FileStorageLoggingDecorator : IFileStorage
    {
        private readonly ILogger<FileStorageLoggingDecorator> logger;
        private readonly string name;

        public FileStorageLoggingDecorator(ILoggerFactory loggerFactory, IFileStorage inner)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(inner, nameof(inner));

            this.logger = loggerFactory.CreateLogger<FileStorageLoggingDecorator>();
            this.Inner = inner;
            this.name = inner.GetType().Name.Replace("FileStorage", string.Empty).ToLower();
        }

        public ISerializer Serializer => this.Inner.Serializer;

        private IFileStorage Inner { get; }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.logger.LogInformation($"{{LogKey:l}} get {this.name} file stream: {path}", LogKeys.FileStorage);
            return await this.Inner.GetFileStreamAsync(path, cancellationToken).AnyContext();
        }

        public async Task<FileInformation> GetFileInformationAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.logger.LogInformation($"{{LogKey:l}} get {this.name} file info: {path}", LogKeys.FileStorage);
            return await this.GetFileInformationAsync(path).AnyContext();
        }

        public async Task<bool> ExistsAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.logger.LogInformation($"{{LogKey:l}} exists {this.name} file: {path}", LogKeys.FileStorage);
            return await this.Inner.ExistsAsync(path).AnyContext();
        }

        public async Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNull(stream, nameof(stream));

            this.logger.LogInformation($"{{LogKey:l}} save {this.name} file: {path} (size={(await ReadBytesAsync(stream).AnyContext()).Length.Bytes().ToString("#.##")})", LogKeys.FileStorage);
            return await this.Inner.SaveFileAsync(path, stream, cancellationToken).AnyContext();
        }

        public async Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(newPath, nameof(newPath));

            this.logger.LogInformation($"{{LogKey:l}} rename {this.name} file: {path} > {newPath}", LogKeys.FileStorage);
            return await this.Inner.RenameFileAsync(path, newPath, cancellationToken).AnyContext();
        }

        public async Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(targetPath, nameof(targetPath));

            this.logger.LogInformation($"{{LogKey:l}} copy {this.name} file: {path} > {targetPath}", LogKeys.FileStorage);
            return await this.Inner.CopyFileAsync(path, targetPath, cancellationToken).AnyContext();
        }

        public async Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.logger.LogInformation($"{{LogKey:l}} delete {this.name} file: {path}", LogKeys.FileStorage);
            return await this.Inner.DeleteFileAsync(path, cancellationToken).AnyContext();
        }

        public async Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey:l}} delete {this.name} files: {searchPattern}", LogKeys.FileStorage);
            return await this.Inner.DeleteFilesAsync(searchPattern, cancellationToken).AnyContext();
        }

        public async Task<PagedResults> GetFileInformationsAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey:l}} get {this.name} file infos: {searchPattern}", LogKeys.FileStorage);
            return await this.GetFileInformationsAsync(pageSize, searchPattern, cancellationToken).AnyContext();
        }

        public void Dispose()
        {
            this.Inner?.Dispose();
        }

        private static async Task<byte[]> ReadBytesAsync(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                await stream.CopyToAsync(ms).AnyContext();
                //stream.Position = 0;
                return ms.ToArray();
            }
        }
    }
}
