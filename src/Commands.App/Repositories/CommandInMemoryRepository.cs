namespace Naos.Core.Commands.App
{
    using System.Collections.Generic;
    using MediatR;
    using Microsoft.Extensions.Logging;
    using Naos.Core.Domain.Repositories;

    public class CommandInMemoryRepository : InMemoryRepository<Command>, ICommandRepository
    {
        // TODO: don't let the repo grow with ALL commands in time > out of mem issue
        public CommandInMemoryRepository(ILogger<IRepository<Command>> logger, IMediator mediator, IEnumerable<Command> entities = null)
            : base(logger, mediator, new InMemoryContext<Command>(entities))
        {
        }
    }
}
