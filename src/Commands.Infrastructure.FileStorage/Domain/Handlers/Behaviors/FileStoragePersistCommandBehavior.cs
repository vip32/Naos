namespace Naos.Core.Commands.Infrastructure.FileStorage
{
    using System.Threading.Tasks;
    using EnsureThat;
    using Naos.Core.Commands.Domain;
    using Naos.Core.Common;
    using Naos.Core.FileStorage.Domain;

    public class FileStoragePersistCommandBehavior : ICommandBehavior
    {
        private readonly IFileStorage fileStorage;
        private readonly ISerializer serializer;
        private readonly string pathTemplate;

        public FileStoragePersistCommandBehavior(IFileStorage filestorage, ISerializer serializer = null, string pathTemplate = "{id}-{type}.json")
        {
            EnsureArg.IsNotNull(filestorage, nameof(filestorage));

            this.fileStorage = filestorage;
            this.serializer = serializer ?? new JsonNetSerializer();
            this.pathTemplate = pathTemplate ?? "{id}-{type}";
        }

        /// <summary>
        /// Executes this behavior for the specified command
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="request">The command.</param>
        /// <returns></returns>
        public async Task<CommandBehaviorResult> ExecuteAsync<TResponse>(CommandRequest<TResponse> request)
        {
            EnsureArg.IsNotNull(request);

            var path = this.pathTemplate
                .Replace("{id}", request.Id)
                .Replace("{type}", request.GetType().PrettyName(false));

            await this.fileStorage.SaveFileObjectAsync(path, request, this.serializer).AnyContext();

            return await Task.FromResult(new CommandBehaviorResult()).AnyContext();
        }
    }
}
