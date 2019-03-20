namespace Naos.Core.Infrastructure.EntityFramework
{
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using Naos.Core.Common;

    public class EntityFrameworkRepositoryOptionsBuilder :
        BaseOptionsBuilder<EntityFrameworkRepositoryOptions, EntityFrameworkRepositoryOptionsBuilder>
    {
        public EntityFrameworkRepositoryOptionsBuilder Mediator(IMediator mediator)
        {
            this.Target.Mediator = mediator;
            return this;
        }

        public EntityFrameworkRepositoryOptionsBuilder Pub(bool publishEvents)
        {
            this.Target.PublishEvents = publishEvents;
            return this;
        }

        public EntityFrameworkRepositoryOptionsBuilder DbContext(DbContext dbContext)
        {
            this.Target.DbContext = dbContext;
            return this;
        }
    }
}