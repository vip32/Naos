namespace Naos.Core.Commands.App.Web
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.FileStorage.Domain;
    using Naos.Foundation;

    public class CommandRequestStorage
    {
        private readonly IFileStorage fileStorage;
        private readonly JsonNetSerializer serializer;

        public CommandRequestStorage(IFileStorage fileStorage)
        {
            EnsureArg.IsNotNull(fileStorage, nameof(fileStorage));

            this.fileStorage = fileStorage;
            this.serializer = new JsonNetSerializer();
        }

        public async Task<bool> SaveAsync(CommandRequestWrapper commandRequest)
        {
            return await this.fileStorage.SaveFileObjectAsync($"{commandRequest.Id}.json", commandRequest, this.serializer).AnyContext();
        }

        public async Task<CommandRequestWrapper> GetAsync(string id)
        {
            return await this.fileStorage.GetFileObjectAsync<CommandRequestWrapper>($"{id}.json", this.serializer).AnyContext();
        }
    }
}
