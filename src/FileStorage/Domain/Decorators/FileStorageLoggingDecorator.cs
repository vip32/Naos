namespace Naos.Core.FileStorage.Domain
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

        public FileStorageLoggingDecorator(ILoggerFactory loggerFactory, IFileStorage decoratee)
        {
            EnsureArg.IsNotNull(loggerFactory, nameof(loggerFactory));
            EnsureArg.IsNotNull(decoratee, nameof(decoratee));

            this.logger = loggerFactory.CreateLogger<FileStorageLoggingDecorator>();
            this.Decoratee = decoratee;
            this.name = decoratee.GetType().Name.Replace("FileStorage", string.Empty).ToLower();
        }

        public ISerializer Serializer => this.Decoratee.Serializer;

        private IFileStorage Decoratee { get; }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.logger.LogInformation($"{{LogKey:l}} get {this.name} file stream: {path}", LogKeys.FileStorage);
            return this.Decoratee.GetFileStreamAsync(path, cancellationToken);
        }

        public async Task<FileInformation> GetFileInformationAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.logger.LogInformation($"{{LogKey:l}} get {this.name} file info: {path}", LogKeys.FileStorage);
            return await this.GetFileInformationAsync(path);
        }

        public Task<bool> ExistsAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.logger.LogInformation($"{{LogKey:l}} exists {this.name} file: {path}", LogKeys.FileStorage);
            return this.Decoratee.ExistsAsync(path);
        }

        public Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNull(stream, nameof(stream));

            this.logger.LogInformation($"{{LogKey:l}} save {this.name} file: {path} (size={ReadBytes(stream).Length.Bytes().ToString("#.##")})", LogKeys.FileStorage);
            return this.Decoratee.SaveFileAsync(path, stream, cancellationToken);
        }

        public Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(newPath, nameof(newPath));

            this.logger.LogInformation($"{{LogKey:l}} rename {this.name} file: {path} > {newPath}", LogKeys.FileStorage);
            return this.Decoratee.RenameFileAsync(path, newPath, cancellationToken);
        }

        public Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(targetPath, nameof(targetPath));

            this.logger.LogInformation($"{{LogKey:l}} copy {this.name} file: {path} > {targetPath}", LogKeys.FileStorage);
            return this.Decoratee.CopyFileAsync(path, targetPath, cancellationToken);
        }

        public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.logger.LogInformation($"{{LogKey:l}} delete {this.name} file: {path}", LogKeys.FileStorage);
            return this.Decoratee.DeleteFileAsync(path, cancellationToken);
        }

        public Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey:l}} delete {this.name} files: {searchPattern}", LogKeys.FileStorage);
            return this.Decoratee.DeleteFilesAsync(searchPattern, cancellationToken);
        }

        public async Task<PagedResults> GetFileInformationsAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default)
        {
            this.logger.LogInformation($"{{LogKey:l}} get {this.name} file infos: {searchPattern}", LogKeys.FileStorage);
            return await this.GetFileInformationsAsync(pageSize, searchPattern, cancellationToken);
        }

        public void Dispose()
        {
            this.Decoratee?.Dispose();
        }

        private static byte[] ReadBytes(Stream stream)
        {
            using(var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                //stream.Position = 0;
                return ms.ToArray();
            }
        }
    }
}
