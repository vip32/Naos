namespace Naos.Commands.App.Web
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.FileStorage.Domain;
    using Naos.Foundation;

    public class CommandRequestStore
    {
        private readonly IFileStorage storage;
        private readonly JsonNetSerializer serializer;

        public CommandRequestStore(IFileStorage storage)
        {
            EnsureArg.IsNotNull(storage, nameof(storage));

            this.storage = storage;
            this.serializer = new JsonNetSerializer(DefaultJsonSerializerSettings.Create());
        }

        public async Task<bool> SaveAsync(CommandRequestWrapper commandRequest)
        {
            return await this.storage.SaveFileObjectAsync($"{commandRequest.Id}.json", commandRequest, this.serializer).AnyContext();
        }

        public async Task<CommandRequestWrapper> GetAsync(string id)
        {
            return await this.storage.GetFileObjectAsync<CommandRequestWrapper>($"{id}.json", this.serializer).AnyContext();
        }
    }
}
