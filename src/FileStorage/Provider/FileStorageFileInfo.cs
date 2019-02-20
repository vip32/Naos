namespace Naos.Core.FileStorage
{
    using System;
    using System.IO;
    using EnsureThat;
    using Microsoft.Extensions.FileProviders;
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;

    public class FileStorageFileInfo : IFileInfo
    {
        private readonly IFileStorage fileStorage;
        private readonly FileInformation file;

        public FileStorageFileInfo(IFileStorage fileStorage, string path)
        {
            EnsureArg.IsNotNull(fileStorage, nameof(fileStorage));

            this.fileStorage = fileStorage;
            this.file = !path.IsNullOrEmpty() ? this.fileStorage.GetFileInformationAsync(path).Result : null;
        }

        public bool Exists => this.file != null;

        public long Length => throw new NotImplementedException();

        public string PhysicalPath => this.file?.Path;

        public string Name => this.file?.Name;

        public DateTimeOffset LastModified => this.file != null ? this.file.Modified : new DateTimeOffset(DateTime.MinValue);

        public bool IsDirectory => false;

        public Stream CreateReadStream()
        {
            if (this.file?.Path.IsNullOrEmpty() == true)
            {
                return null;
            }

            return this.fileStorage.GetFileStreamAsync(this.file.Path).Result;
        }
    }
}
