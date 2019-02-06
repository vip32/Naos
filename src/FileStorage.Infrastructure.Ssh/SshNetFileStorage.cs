namespace Naos.Core.FileStorage.Infrastructure.Ssh
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
    using Naos.Core.Common;
    using Naos.Core.Common.Serialization;
    using Naos.Core.FileStorage.Domain;
    using Renci.SshNet;
    using Renci.SshNet.Common;

    public class SshNetFileStorage : IFileStorage
    {
        private readonly ILogger<SshNetFileStorage> logger;
        private readonly ConnectionInfo connectionInfo;
        private readonly SftpClient client;

        public SshNetFileStorage(ILogger<SshNetFileStorage> logger, SshNetFileStorageOptions options)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));

            this.logger = logger;
            options = options ?? new SshNetFileStorageOptions();
            this.Serializer = options.Serializer ?? DefaultSerializer.Instance;

            this.connectionInfo = this.CreateConnectionInfo(options);
            this.client = new SftpClient(this.connectionInfo);
        }

        public ISerializer Serializer { get; }

        public async Task<Stream> GetFileStreamAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.EnsureClientConnected();
            try
            {
                var stream = new MemoryStream();
                await Task.Factory.FromAsync(this.client.BeginDownloadFile(this.NormalizePath(path), stream, null, null), this.client.EndDownloadFile).AnyContext();
                stream.Seek(0, SeekOrigin.Begin);

                return stream;
            }
            catch (SftpPathNotFoundException ex)
            {
                if (this.logger.IsEnabled(LogLevel.Trace))
                {
                    this.logger.LogTrace(ex, "Error trying to get file stream: {Path}", path);
                }

                return null;
            }
        }

        public Task<FileInformation> GetFileInformationAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.EnsureClientConnected();
            try
            {
                var file = this.client.Get(this.NormalizePath(path));
                return Task.FromResult(new FileInformation
                {
                    Path = file.FullName.TrimStart('/'),
                    Created = file.LastWriteTimeUtc,
                    Modified = file.LastWriteTimeUtc,
                    Size = file.Length
                });
            }
            catch (SftpPathNotFoundException ex)
            {
                if (this.logger.IsEnabled(LogLevel.Trace))
                {
                    this.logger.LogTrace(ex, "Error trying to getting file info: {Path}", path);
                }

                return Task.FromResult<FileInformation>(null);
            }
        }

        public Task<bool> ExistsAsync(string path)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.EnsureClientConnected();
            return Task.FromResult(this.client.Exists(this.NormalizePath(path)));
        }

        public async Task<bool> SaveFileAsync(string path, Stream stream, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNull(stream, nameof(stream));

            path = this.NormalizePath(path);
            this.EnsureClientConnected();
            this.EnsureDirectoryExists(path);

            await Task.Factory.FromAsync(this.client.BeginUploadFile(stream, path, null, null), this.client.EndUploadFile).AnyContext();

            return true;
        }

        public Task<bool> RenameFileAsync(string path, string newPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(newPath, nameof(newPath));

            newPath = this.NormalizePath(newPath);
            this.EnsureClientConnected();
            this.EnsureDirectoryExists(newPath);
            this.client.RenameFile(this.NormalizePath(path), newPath, true);

            return Task.FromResult(true);
        }

        public async Task<bool> CopyFileAsync(string path, string targetPath, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));
            EnsureArg.IsNotNullOrEmpty(targetPath, nameof(targetPath));

            using (var stream = await this.GetFileStreamAsync(path, cancellationToken).AnyContext())
            {
                if (stream == null)
                {
                    return false;
                }

                return await this.SaveFileAsync(targetPath, stream, cancellationToken).AnyContext();
            }
        }

        public Task<bool> DeleteFileAsync(string path, CancellationToken cancellationToken = default)
        {
            EnsureArg.IsNotNullOrEmpty(path, nameof(path));

            this.EnsureClientConnected();
            try
            {
                this.client.DeleteFile(this.NormalizePath(path));
            }
            catch (SftpPathNotFoundException ex)
            {
                this.logger.LogDebug(ex, "Error trying to delete file: {Path}.", path);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }

        public async Task<int> DeleteFilesAsync(string searchPattern = null, CancellationToken cancellationToken = default)
        {
            var files = await this.GetFileListAsync(searchPattern, cancellationToken: cancellationToken).AnyContext();
            int count = 0;

            foreach (var file in files) // batch?
            {
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

            searchPattern = this.NormalizePath(searchPattern);

            var result = new PagedResults(() => this.GetFiles(searchPattern, 1, pageSize, cancellationToken));
            await result.NextPageAsync().AnyContext();
            return result;
        }

        public void Dispose()
        {
            if (this.client.IsConnected)
            {
                this.client.Disconnect();
            }

            this.client.Dispose();
        }

        private async Task<NextPageResult> GetFiles(string searchPattern, int page, int pageSize, CancellationToken cancellationToken)
        {
            int pagingLimit = pageSize;
            int skip = (page - 1) * pagingLimit;
            if (pagingLimit < int.MaxValue)
            {
                pagingLimit = pagingLimit + 1;
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

        private async Task<IEnumerable<FileInformation>> GetFileListAsync(string searchPattern = null, int? limit = null, int? skip = null, CancellationToken cancellationToken = default)
        {
            if (limit.HasValue && limit.Value <= 0)
            {
                return new List<FileInformation>();
            }

            var list = new List<FileInformation>();
            var criteria = this.GetRequestCriteria(this.NormalizePath(searchPattern));

            this.EnsureClientConnected();
            if (!string.IsNullOrEmpty(criteria.Prefix) && !this.client.Exists(criteria.Prefix))
            {
                return list;
            }

            // NOTE: This could be very expensive the larger the directory structure you have as we aren't efficiently doing paging.
            await this.GetFileListRecursivelyAsync(criteria.Prefix, criteria.Pattern, list).AnyContext();

            if (skip.HasValue)
            {
                list = list.Skip(skip.Value).ToList();
            }

            if (limit.HasValue)
            {
                list = list.Take(limit.Value).ToList();
            }

            return list;
        }

        private async Task GetFileListRecursivelyAsync(string prefix, Regex pattern, List<FileInformation> list)
        {
            var files = await Task.Factory.FromAsync(this.client.BeginListDirectory(prefix, null, null), this.client.EndListDirectory).AnyContext();
            foreach (var file in files)
            {
                if (file.IsDirectory)
                {
                    if (file.Name == "." || file.Name == "..")
                    {
                        continue;
                    }

                    await this.GetFileListRecursivelyAsync(string.Concat(prefix, "/", file.Name), pattern, list).AnyContext();
                    continue;
                }

                if (!file.IsRegularFile)
                {
                    continue;
                }

                string path = file.FullName.TrimStart('/');
                if (pattern != null && !pattern.IsMatch(path))
                {
                    continue;
                }

                list.Add(new FileInformation
                {
                    Path = path,
                    Created = file.LastWriteTimeUtc,
                    Modified = file.LastWriteTimeUtc,
                    Size = file.Length
                });
            }
        }

        private ConnectionInfo CreateConnectionInfo(SshNetFileStorageOptions options)
        {
            if (string.IsNullOrEmpty(options.ConnectionString))
            {
                throw new ArgumentNullException(nameof(options.ConnectionString));
            }

            if (!Uri.TryCreate(options.ConnectionString, UriKind.Absolute, out var uri) || string.IsNullOrEmpty(uri?.UserInfo))
            {
                throw new ArgumentException("Unable to parse connection string uri", nameof(options.ConnectionString));
            }

            string[] userParts = uri.UserInfo.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            string username = Uri.UnescapeDataString(userParts.First());
            string password = Uri.UnescapeDataString(userParts.Length > 0 ? userParts[1] : string.Empty);
            int port = uri.Port > 0 ? uri.Port : 22;

            var authenticationMethods = new List<AuthenticationMethod>();
            if (!string.IsNullOrEmpty(password))
            {
                authenticationMethods.Add(new PasswordAuthenticationMethod(username, password));
            }

            if (options.PrivateKey != null)
            {
                authenticationMethods.Add(new PrivateKeyAuthenticationMethod(username, new PrivateKeyFile(options.PrivateKey, options.PrivateKeyPassPhrase)));
            }

            if (authenticationMethods.Count == 0)
            {
                authenticationMethods.Add(new NoneAuthenticationMethod(username));
            }

            if (!string.IsNullOrEmpty(options.Proxy))
            {
                if (!Uri.TryCreate(options.Proxy, UriKind.Absolute, out var proxyUri) || string.IsNullOrEmpty(proxyUri?.UserInfo))
                {
                    throw new ArgumentException("Unable to parse proxy uri", nameof(options.Proxy));
                }

                string[] proxyParts = proxyUri.UserInfo.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                string proxyUsername = proxyParts.First();
                string proxyPassword = proxyParts.Length > 0 ? proxyParts[1] : null;

                var proxyType = options.ProxyType;
                if (proxyType == ProxyTypes.None && proxyUri.Scheme != null && proxyUri.Scheme.StartsWith("http"))
                {
                    proxyType = ProxyTypes.Http;
                }

                return new ConnectionInfo(uri.Host, port, username, proxyType, proxyUri.Host, proxyUri.Port, proxyUsername, proxyPassword, authenticationMethods.ToArray());
            }

            return new ConnectionInfo(uri.Host, port, username, authenticationMethods.ToArray());
        }

        private void EnsureClientConnected()
        {
            if (!this.client.IsConnected)
            {
                this.client.Connect();
            }
        }

        private void EnsureDirectoryExists(string path)
        {
            string directory = this.NormalizePath(Path.GetDirectoryName(path));
            if (string.IsNullOrEmpty(directory) || this.client.Exists(directory))
            {
                return;
            }

            string[] folderSegments = directory.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string currentDirectory = string.Empty;
            foreach (string segment in folderSegments)
            {
                currentDirectory = string.Concat(currentDirectory, "/", segment);
                if (!this.client.Exists(currentDirectory))
                {
                    this.client.CreateDirectory(currentDirectory);
                }
            }
        }

        private string NormalizePath(string path)
        {
            return path?.Replace('\\', '/');
        }

        private SearchCriteria GetRequestCriteria(string searchPattern)
        {
            Regex patternRegex = null;
            searchPattern = searchPattern?.Replace('\\', '/');

            string prefix = searchPattern;
            int wildcardPos = searchPattern?.IndexOf('*') ?? -1;
            if (searchPattern != null && wildcardPos >= 0)
            {
                patternRegex = new Regex("^" + Regex.Escape(searchPattern).Replace("\\*", ".*?") + "$");
                int slashPos = searchPattern.LastIndexOf('/');
                prefix = slashPos >= 0 ? searchPattern.Substring(0, slashPos) : string.Empty;
            }

            return new SearchCriteria
            {
                Prefix = prefix ?? string.Empty,
                Pattern = patternRegex
            };
        }

        private class SearchCriteria
        {
            public string Prefix { get; set; }

            public Regex Pattern { get; set; }
        }
    }
}
