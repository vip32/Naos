namespace Naos.Core.App.Commands
{
    using Naos.Core.Domain.Repositories;

    public interface ICommandRequestRepository : IRepository<CommandEntity>
    {
    }
}
