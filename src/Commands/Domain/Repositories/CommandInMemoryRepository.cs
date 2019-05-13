namespace Naos.Core.Commands.Domain
{
    using Naos.Core.Common;
    using Naos.Core.Domain.Repositories;

    public class CommandInMemoryRepository : InMemoryRepository<Command>, ICommandRepository
    {
        // TODO: don't let the repo grow with ALL commands in time > out of mem issue

        public CommandInMemoryRepository(InMemoryRepositoryOptions<Command> options)
            : base(options)
        {
        }

        public CommandInMemoryRepository(Builder<InMemoryRepositoryOptionsBuilder<Command>, InMemoryRepositoryOptions<Command>> optionsBuilder)
            : base(optionsBuilder)
        {
        }
    }
}
