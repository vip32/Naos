namespace Naos.Core.FileStorage.Infrastructure
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;
    using Naos.Core.FileStorage.Domain;

    public class EmbeddedFileStorage : IFileStorage
    {
        private readonly ILogger<EmbeddedFileStorage> logger;
        private readonly object @lock = new object();
        private readonly ManifestEmbeddedFileProvider provider;
        private readonly EmbeddedFileStorageOptions options;

        public EmbeddedFileStorage(EmbeddedFileStorageOptions options)
        {
            EnsureArg.IsNotNull(options.LoggerFactory, nameof(options.LoggerFactory));

            this.logger = options.LoggerFactory.CreateLogger<EmbeddedFileStorage>();
            this.options = options ?? new EmbeddedFileStorageOptions();
            this.options.Assembly = options.Assembly ?? Assembly.GetEntryAssembly();
            this.Serializer = options.Serializer ?? DefaultSerializer.Instance;

            this.provider = new ManifestEmbeddedFileProvider(this.options.Assembly);
        }

        public EmbeddedFileStorage(Builder<EmbeddedFileStorageOptionsBuilder, EmbeddedFileStorageOptions> config)
            : this(config(new EmbeddedFileStorageOptionsBuilder()).Build())
        {
        }

        public ISerializer Serializer { get; }

        public Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            try
            {
                return Task.FromResult(this.provider.GetFileInfo(path).CreateReadStream());
            }
            catch (IOException ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                if (this.logger.IsEnabled(LogLevel.Warning))
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
            if (!this.provider.GetFileInfo(path).Exists)
            {
                return Task.FromResult<FileInformation>(null);
            }

            return Task.FromResult(
                new FileInformation
                {
                    Path = path,
                    Name = path.SubstringFromLast(Path.DirectorySeparatorChar.ToString()),
                    Created = this.options.Assembly.GetLinkerDateTime(),
                    Modified = this.options.Assembly.GetLinkerDateTime(),
                    //Size = info.Length
                });
        }

        public Task<bool> ExistsAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            return Task.FromResult(this.provider.GetFileInfo(path).Exists);
        }

        public Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<PagedResults> GetPagedFileListAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
