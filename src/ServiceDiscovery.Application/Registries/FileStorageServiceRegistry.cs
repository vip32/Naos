namespace Naos.ServiceDiscovery.Application
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.FileStorage.Domain;
    using Naos.Foundation;
    using Newtonsoft.Json;

    public class FileStorageServiceRegistry : IServiceRegistry
    {
        private readonly ILogger<FileStorageServiceRegistry> logger;
        private readonly IFileStorage fileStorage;
        private readonly List<ServiceRegistration> registrations = new List<ServiceRegistration>();
        private string directory;
        private FileSystemWatcher watcher;

        public FileStorageServiceRegistry(
            ILogger<FileStorageServiceRegistry> logger,
            IFileStorage fileStorage)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(fileStorage, nameof(fileStorage));

            this.logger = logger;
            this.fileStorage = fileStorage;

            // TODO: inject HealthStrategy which can validate the registrations
            this.logger.LogInformation($"{{LogKey:l}} filestorage active (type={this.fileStorage.GetType().Name})", LogKeys.ServiceDiscovery);
            this.directory = "OBSOLETE";
        }

        public Task DeRegisterAsync(string id)
        {
            EnsureArg.IsNotNullOrEmpty(id, nameof(id));

            var path = Path.Combine(this.directory, $"registration_{id}.json.tmp");
            if (File.Exists(path))
            {
                this.logger.LogInformation("{LogKey:l} filesystem registration delete (id={RegistrationId})", LogKeys.ServiceDiscovery, id);

                File.Delete(path);
            }

            return Task.CompletedTask;
        }

        public Task RegisterAsync(ServiceRegistration registration)
        {
            EnsureArg.IsNotNull(registration, nameof(registration));

            // store message in specific (Discovery) folder
            this.EnsureDirectory(this.directory);

            var pathTemp = Path.Combine(this.directory, $"registration_{registration.Id}.json.tmp");
            var path = Path.Combine(this.directory, $"registration_{registration.Id}.json");
            this.logger.LogInformation($"{{LogKey:l}} register filesystem (name={{RegistrationName}}, tags={string.Join("|", registration.Tags.Safe())}, id={{RegistrationId}}, address={registration.FullAddress}, file={path.SliceFromLast(@"\")})",
                LogKeys.ServiceDiscovery, registration.Name, registration.Id);

            if (File.Exists(pathTemp))
            {
                File.Delete(pathTemp);
            }

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var streamWriter = File.CreateText(pathTemp))
            {
                streamWriter.Write(SerializationHelper.JsonSerialize(registration));
                streamWriter.Flush();
            }

            File.Move(pathTemp, path); // rename file

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<ServiceRegistration>> RegistrationsAsync()
        {
            if (this.watcher == null)
            {
                this.EnsureDirectory(this.directory);
                this.RefreshRegistrations(this.directory);
                this.logger.LogInformation($"{{LogKey:l}} filesystem registrations watch (folder={this.directory})", LogKeys.ServiceDiscovery);

                this.watcher = new FileSystemWatcher(this.directory)
                {
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                    EnableRaisingEvents = true,
                    Filter = "*.json"
                };

                this.watcher.Renamed += (sender, e) =>
                {
                    this.RefreshRegistrations(this.directory);
                };

                this.watcher.Changed += (sender, e) =>
                {
                    this.RefreshRegistrations(this.directory);
                };

                this.watcher.Deleted += (sender, e) =>
                {
                    this.RefreshRegistrations(this.directory);
                };
            }

            return await Task.Run(() => this.registrations.DistinctBy(r => r.Id)).AnyContext();
        }

        private string GetDirectory(FileSystemServiceRegistryConfiguration configuration)
        {
            return configuration.Folder.EmptyToNull() ?? Path.GetTempPath();
        }

        private void EnsureDirectory(string fullPath)
        {
            this.logger.LogInformation($"EnsureDirectory #1 {Directory.Exists(fullPath)} {fullPath}");
            if (!Directory.Exists(fullPath))
            {
                this.logger.LogInformation("EnsureDirectory #2");
                var directory = Directory.CreateDirectory(fullPath);
                this.logger.LogInformation("EnsureDirectory #3");
                this.logger.LogWarning($"{{LogKey:l}} filesystem folder created (folder={fullPath}, exists={directory.Exists})", LogKeys.ServiceDiscovery);
            }

            this.logger.LogInformation("EnsureDirectory #4");
            if (!Directory.Exists(fullPath))
            {
                this.logger.LogWarning($"{{LogKey:l}} filesystem folder could not be created (folder={fullPath})", LogKeys.ServiceDiscovery);
            }

            this.logger.LogInformation("EnsureDirectory #5");
        }

        private string GetFileContents(string fullPath)
        {
            //System.Threading.Thread.Sleep(200); // this helps with locked files
            using (var reader = new StreamReader(File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                return reader.ReadToEnd();
            }
        }

        private void RefreshRegistrations(string directory)
        {
            this.registrations.Clear();

            foreach (var path in Directory.GetFiles(directory))
            {
                var registration = JsonConvert.DeserializeObject<ServiceRegistration>(this.GetFileContents(path));
                if (registration != null)
                {
                    this.logger.LogInformation($"{{LogKey:l}} filesystem registrations refresh (name={{RegistrationName}}, id={{RegistrationId}}, file={path.SliceFromLast(@"\")})",
                        LogKeys.ServiceDiscovery, registration.Name, registration.Id);
                    this.registrations.Add(registration);
                }
            }
        }
    }
}
