namespace Naos.Core.App.Commands
{
    using Naos.Core.Domain;

    public interface ICommandRequestRepository : IRepository<CommandEntity, string>
    {
    }
}
