namespace Naos.Core.App.Commands
{
    using System;
    using System.Collections.Generic;
    using MediatR;
    using Naos.Core.Domain;

    public class CommandEntityInMemoryRepository : InMemoryRepository<CommandEntity>, ICommandRequestRepository
    {
        // TODO: don't let the repo grow with ALL commands in time > out of mem issue
        public CommandEntityInMemoryRepository(IMediator mediator, IEnumerable<CommandEntity> entities = null)
            : base(mediator, entities)
        {
        }
    }
}
