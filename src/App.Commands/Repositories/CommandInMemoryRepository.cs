namespace Naos.Core.App.Commands
{
    using System.Collections.Generic;
    using MediatR;
    using Naos.Core.Domain.Repositories;

    public class CommandInMemoryRepository : InMemoryRepository<Command>, ICommandRepository
    {
        // TODO: don't let the repo grow with ALL commands in time > out of mem issue
        public CommandInMemoryRepository(IMediator mediator, IEnumerable<Command> entities = null)
            : base(mediator, entities)
        {
        }
    }
}
