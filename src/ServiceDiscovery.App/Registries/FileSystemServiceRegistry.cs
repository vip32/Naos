namespace Naos.Core.ServiceDiscovery.App
{
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;
    using EnsureThat;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Common;
    using Newtonsoft.Json;

    public class FileSystemServiceRegistry : IServiceRegistry
    {
        private readonly ILogger<FileSystemServiceRegistry> logger;
        private readonly FileSystemServiceRegistryConfiguration configuration;
        private readonly string directory;
        private List<ServiceRegistration> registrations = new List<ServiceRegistration>();
        private FileSystemWatcher watcher;

        public FileSystemServiceRegistry(
            ILogger<FileSystemServiceRegistry> logger,
            FileSystemServiceRegistryConfiguration configuration)
        {
            EnsureArg.IsNotNull(logger, nameof(logger));
            EnsureArg.IsNotNull(configuration, nameof(configuration));

            this.logger = logger;
            this.configuration = configuration ?? new FileSystemServiceRegistryConfiguration();
            this.directory = this.GetDirectory(this.configuration);

            // TODO: inject HealthStrategy which can validate the registrations
            this.logger.LogInformation("{LogKey} filesystem active", LogEventKeys.ServiceDiscovery);
        }

        public Task DeRegisterAsync(string id)
        {
            EnsureArg.IsNotNullOrEmpty(id, nameof(id));

            var fullFileName = Path.Combine(this.GetDirectory(this.configuration), $"registration_{id}.json.tmp");
            if (File.Exists(fullFileName))
            {
                this.logger.LogInformation("{LogKey} filesystem registration delete (id={RegistrationId})", LogEventKeys.ServiceDiscovery, id);

                File.Delete(fullFileName);
            }

            return Task.CompletedTask;
        }

        public Task RegisterAsync(ServiceRegistration registration)
        {
            EnsureArg.IsNotNull(registration, nameof(registration));

            // store message in specific (Discovery) folder
            this.EnsureDirectory(this.directory);

            var fullFileNameTemp = Path.Combine(this.GetDirectory(this.configuration), $"registration_{registration.Id}.json.tmp");
            var fullFileName = Path.Combine(this.GetDirectory(this.configuration), $"registration_{registration.Id}.json");
            this.logger.LogInformation($"{{LogKey}} register filesystem (name={{RegistrationName}}, tags={string.Join("|", registration.Tags.Safe())}, id={{RegistrationId}}, address={registration.FullAddress}, file={fullFileName.SubstringFromLast(@"\")})",
                LogEventKeys.ServiceDiscovery, registration.Name, registration.Id);

            if (File.Exists(fullFileNameTemp))
            {
                File.Delete(fullFileNameTemp);
            }

            if (File.Exists(fullFileName))
            {
                File.Delete(fullFileName);
            }

            using (var streamWriter = File.CreateText(fullFileNameTemp))
            {
                streamWriter.Write(SerializationHelper.JsonSerialize(registration));
                streamWriter.Flush();
            }

            File.Move(fullFileNameTemp, fullFileName); // rename file

            return Task.CompletedTask;
        }

        public async Task<IEnumerable<ServiceRegistration>> RegistrationsAsync()
        {
            if (this.watcher == null)
            {
                this.EnsureDirectory(this.directory);
                this.RefreshRegistrations(this.directory);
                this.logger.LogInformation($"{{LogKey}} filesystem registrations watch (directory={this.directory})", LogEventKeys.ServiceDiscovery);

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

            return await Task.Run(() => this.registrations.DistinctBy(r => r.Id)).ConfigureAwait(false);
        }

        private string GetDirectory(FileSystemServiceRegistryConfiguration configuration)
        {
            return $@"{configuration.Folder.EmptyToNull() ?? Path.GetTempPath()}naos_servicediscovery\".Replace("\\", @"\");
        }

        private void EnsureDirectory(string fullPath)
        {
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
        }

        private string ReadFile(string fullPath)
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

            foreach (var fullPath in Directory.GetFiles(directory))
            {
                var registration = JsonConvert.DeserializeObject<ServiceRegistration>(this.ReadFile(fullPath));
                if (registration != null)
                {
                    this.logger.LogInformation($"{{LogKey}} filesystem registrations refresh (name={{RegistrationName}}, id={{RegistrationId}}, file={fullPath.SubstringFromLast(@"\")})",
                        LogEventKeys.ServiceDiscovery, registration.Name, registration.Id);
                    this.registrations.Add(registration);
                }
            }
        }
    }
}
