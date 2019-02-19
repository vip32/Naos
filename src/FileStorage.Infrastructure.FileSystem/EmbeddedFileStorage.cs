namespace Naos.Core.FileStorage.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;
    using Naos.Core.Domain.Model;
    using Naos.Core.FileStorage.Domain;

    public class EmbeddedFileStorage : IFileStorage
    {
        private readonly ILogger<EmbeddedFileStorage> logger;
        private readonly object @lock = new object();
        private readonly EmbeddedFileStorageOptions options;

        public EmbeddedFileStorage(EmbeddedFileStorageOptions options)
        {
            EnsureArg.IsNotNull(options.LoggerFactory, nameof(options.LoggerFactory));

            this.logger = options.LoggerFactory.CreateLogger<EmbeddedFileStorage>();
            this.options = options ?? new EmbeddedFileStorageOptions();
            this.options.Assemblies = options.Assemblies ?? new[] { Assembly.GetEntryAssembly() };
            this.Serializer = options.Serializer ?? DefaultSerializer.Instance;
        }

        public EmbeddedFileStorage(Builder<EmbeddedFileStorageOptionsBuilder, EmbeddedFileStorageOptions> config)
            : this(config(new EmbeddedFileStorageOptionsBuilder()).Build())
        {
        }

        public ISerializer Serializer { get; }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            if (!await this.ExistsAsync(path))
            {
                return null;
            }

            var item = this.GetResourceItems(this.options.Assemblies)
                .FirstOrDefault(p => p.Path.SafeEquals(path));

            if(item == null)
            {
                return null;
            }

            return item.Assembly.GetManifestResourceStream(item.Name);
        }

        public async Task<FileInformation> GetFileInformationAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            if (!await this.ExistsAsync(path))
            {
                return null;
            }

            var item = this.GetResourceItems(this.options.Assemblies)
                .FirstOrDefault(p => p.Path.SafeEquals(path));

            if (item == null)
            {
                return null;
            }

            FileInformation result = this.Map(item);

            return result;
        }

        public Task<bool> ExistsAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            path = PathHelper.Normalize(path);
            return Task.FromResult(
                this.GetResourceItems(this.options.Assemblies).SafeAny(p => p.Path.SafeEquals(path)));
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

        public Task<PagedResults> GetFileInformationsAsync(int pageSize = 100, string searchPattern = null, CancellationToken cancellationToken = default)
        {
            var items = this.GetResourceItems(this.options.Assemblies);

            if (items.IsNullOrEmpty())
            {
                return null;
            }

            if (!searchPattern.IsNullOrEmpty())
            {
                items = items.Where(i => i.Path.EqualsWildcard(searchPattern));
            }

            return Task.FromResult(new PagedResults(
                items.Select(i => this.Map(i)).ToList().AsReadOnly()));
        }

        public void Dispose()
        {
        }

        private IEnumerable<ResourceItem> GetResourceItems(IEnumerable<Assembly> assemblies)
        {
            var result = new List<ResourceItem>();
            foreach (var assembly in assemblies.Safe())
            {
                var created = assembly.GetBuildDate();
                result.AddRange(
                    assembly.GetManifestResourceNames()
                            .Select(r => new ResourceItem
                            {
                                Path = PathHelper.Normalize(r.SubstringTillLast(".").Replace(".", Path.DirectorySeparatorChar.ToString()) + "." + r.SubstringFromLast(".")),
                                Name = r,
                                Created = created,
                                Assembly = assembly
                            }));
            }

            return result;
        }

        private FileInformation Map(ResourceItem item)
        {
            var result = new FileInformation
            {
                Path = item.Path,
                Name = item.Path.SubstringFromLast(Path.DirectorySeparatorChar.ToString()),
                Created = item.Created,
                Modified = item.Created,
                //Size = info.Length
            };
            result.Properties.AddOrUpdate("resourceName", item.Name);
            result.Properties.AddOrUpdate("assembly", item.Assembly.GetName().Name);
            return result;
        }

        private class ResourceItem
        {
            /// <summary>
            /// Gets or sets the path.
            /// </summary>
            /// <value>
            /// The path.
            /// </value>
            public string Path { get; set; }

            /// <summary>
            /// Gets or sets the name of the resource.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            public DateTime Created { get; set; }

            public Assembly Assembly { get; set; }
        }
    }
}
